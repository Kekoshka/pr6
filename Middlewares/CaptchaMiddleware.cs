using Microsoft.Extensions.Caching.Memory;
using pr6.Interfaces;
using System.Net;
using System.Net.Mime;

namespace pr6.Middlewares
{
    public class CaptchaMiddleware
    {
        readonly RequestDelegate _next;
        IMemoryCache _memoryCache;
        ICaptchaService _captchaService;
        public CaptchaMiddleware(RequestDelegate next,
            IMemoryCache memoryCache,
            ICaptchaService captchaService)
        {
            _next = next;
            _memoryCache = memoryCache;
            _captchaService = captchaService;
        }
        public void Invoke(HttpContext context)
        {
            var userIp = context.Connection.RemoteIpAddress;
            _memoryCache.TryGetValue(userIp, out bool isPassedCaptcha);
            if (isPassedCaptcha) _next(context); 
            else RedirectToVerification(context);
        }
        private Task RedirectToVerification(HttpContext context)
        {
            var captcha = _captchaService.GenerateCaptcha();
            _memoryCache.Set("Captcha_" + context.Connection.RemoteIpAddress, false);

            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            context.Response.ContentType = "application/json";
            var response = new
            {
                error = "Необходимо пройти капчу!",
                captchaId = captcha.Id,
                captcha = captcha.Image
            };
            return context.Response.WriteAsJsonAsync(response);
        }
    }
}
