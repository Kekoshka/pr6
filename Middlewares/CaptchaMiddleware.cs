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
        IRequestService _requestService;
        public CaptchaMiddleware(RequestDelegate next,
            IMemoryCache memoryCache,
            ICaptchaService captchaService,
            IRequestService requestService)
        {
            _next = next;
            _memoryCache = memoryCache;
            _captchaService = captchaService;
            _requestService = requestService;
        }
        public void Invoke(HttpContext context)
        {
            if (IsNeedToSolveCaptcha(context))
            {
                _requestService.SaveRequestInCache(context);
                RedirectToVerification(context);
            }
        }
        private Task RedirectToVerification(HttpContext context)
        {
            var captcha = _captchaService.GenerateCaptcha();

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
        private bool IsNeedToSolveCaptcha(HttpContext context)
        {
            return true;
        }
    }
}
