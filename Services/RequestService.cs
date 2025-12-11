using System.Text;
using Microsoft.Extensions.Caching.Memory;
using pr6.Interfaces;
using pr6.Models;

namespace pr6.Services
{
    public class RequestService : IRequestService
    {
        IMemoryCache _memoryCache;
        IHttpClientFactory _httpClientFactory;
        IHttpContextAccessor _httpContextAccessor;
        public RequestService(IMemoryCache memoryCache,
            IHttpClientFactory httpClientFactory,
            IHttpContextAccessor httpContextAccessor) 
        {
            _memoryCache = memoryCache;
            _httpClientFactory = httpClientFactory;
            _httpContextAccessor = httpContextAccessor;
        }
        public async Task<string> SaveRequestInCacheAsync(HttpContext context, CancellationToken cancellationToken)
        {
            var requestId = Guid.NewGuid().ToString();

            context.Response.Cookies.Append("pending_request_id", requestId, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict
            });

            string? body = null;
            if(context.Request.ContentLength > 0)
            {
                context.Request.EnableBuffering();
                using var reader = new StreamReader(
                    context.Request.Body,
                    Encoding.UTF8,
                    detectEncodingFromByteOrderMarks: false,
                    bufferSize: 1024,
                    leaveOpen: true);

                body = await reader.ReadToEndAsync(cancellationToken);
                context.Request.Body.Position = 0;
            }

            var requestData = new HttpContextData
            {
                Method = context.Request.Method,
                Path = context.Request.Path,
                QueryString = context.Request.QueryString.ToString(),
                Body = body,
                ContentType = context.Request.ContentType,
                Scheme = context.Request.Scheme,
                Host = context.Request.Host.ToString()
            };
            
            foreach(var header in context.Request.Headers)
                if(!header.Key.Equals("Cookie", StringComparison.OrdinalIgnoreCase))
                    requestData.Headers[header.Key] = header.Value.ToString();

            _memoryCache.Set($"captcha_request_{requestId}", requestData, TimeSpan.FromMinutes(10));
            return requestId;
        }

        public Task<HttpContextData> GetRequestFromCacheAsync(string requestId, CancellationToken cancellationToken)
        {
            var cacheKey = $"captcha_request_{requestId}";

            if(_memoryCache.TryGetValue<HttpContextData>(cacheKey, out var requestData))
            {
                _memoryCache.Remove(cacheKey);
                return Task.FromResult(requestData);
            }

            throw new KeyNotFoundException("Request was not found in the cache or timeout period has expired");
        }
        public async Task ExecuteCachedRequestAsync(string requestId, HttpContext currentContext, CancellationToken cancellationToken)
        {
            var requestData = await GetRequestFromCacheAsync(requestId, cancellationToken);

            using var httpClient = _httpClientFactory.CreateClient("captcha");

            var requestUri = $"{requestData.Scheme}://{requestData.Host}{requestData.Path}{requestData.QueryString}";

            var requestMessage = new HttpRequestMessage(
                new HttpMethod(requestData.Method),
                requestUri);

            foreach (var header in requestData.Headers)
                if (!requestMessage.Headers.TryAddWithoutValidation(header.Key, header.Value))
                    requestMessage.Content?.Headers.TryAddWithoutValidation(header.Key, header.Value);

            requestMessage.Headers.Add("is-server", "true");

            if (!string.IsNullOrEmpty(requestData.Body))
            {
                var mediaType = requestData.ContentType ?? "application/json";
                requestMessage.Content = new StringContent(requestData.Body, Encoding.UTF8, mediaType);
            }

            var response = await httpClient.SendAsync(requestMessage, cancellationToken);

            currentContext.Response.StatusCode = (int)response.StatusCode;

            foreach(var header in response.Headers)
                currentContext.Response.Headers[header.Key] = header.Value.ToArray();

            foreach (var header in response.Content.Headers)
                currentContext.Response.Headers[header.Key] = header.Value.ToArray();

            if(response.StatusCode != System.Net.HttpStatusCode.NoContent)
            {
                var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);
                await currentContext.Response.WriteAsync(responseBody, cancellationToken);
            }
        }
    }
}
