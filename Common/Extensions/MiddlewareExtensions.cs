using pr6.Middlewares;

namespace pr6.Common.Extensions
{
    public static class MiddlewareExtensions
    {
        public static IApplicationBuilder UseExceptionHandling(this IApplicationBuilder builder) => 
            builder.UseMiddleware<ExceptionHandlingMiddleware>();
        public static IApplicationBuilder UseCaptcha(this IApplicationBuilder builder) =>
            builder.UseMiddleware<CaptchaMiddleware>();
    }
}
