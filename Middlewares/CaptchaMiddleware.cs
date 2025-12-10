using Microsoft.Extensions.Caching.Memory;
using pr6.Interfaces;
using System.Net;
using System.Net.Mime;
using System.Threading.Tasks;

namespace pr6.Middlewares
{
    public class CaptchaMiddleware
    {
        readonly RequestDelegate _next;
        public CaptchaMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(
            HttpContext context,
            IMemoryCache memoryCache,
            ICaptchaService captchaService,
            IRequestService requestService)
        {
            var cancellationToken = context.RequestAborted;
            if (IsNeedToSolveCaptcha(context))
            {
                var requestId = await requestService.SaveRequestInCacheAsync(context, cancellationToken);
                await RedirectToVerification(context, requestId, captchaService);
                return;
            }

            await _next(context);
        }
        private Task RedirectToVerification(HttpContext context, string requestId, ICaptchaService captchaService )
        {
            var captcha = captchaService.GenerateCaptcha(requestId);

            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            context.Response.ContentType = "application/json";
            var response = new
            {
                error = "Необходимо пройти капчу!",
                requestId,
                captcha = captcha.Image
            };
            return context.Response.WriteAsJsonAsync(response);
        }
        private bool IsNeedToSolveCaptcha(HttpContext context)
        {
            List<string> Methods = new()
            {
                "StartAuthenticationAsync",
                "StartRecoverPasswordAsync",
                "StartRegistrationAsync"
            };
            if(Methods.Any(m => context.Request.Path.ToString().Contains(m)) && !context.Request.Headers.Any(h => h.Key == "is-server" && h.Value == "true"))
                return true;
            return false;
        }
    }
}
