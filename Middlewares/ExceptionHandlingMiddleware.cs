using Microsoft.Extensions.Options;
using pr6.Common.CustomExceptions;
using pr6.Models.Options;

namespace pr6.Middlewares
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;

        public ExceptionHandlingMiddleware(
            RequestDelegate next, 
            ILogger<ExceptionHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }
        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An exception occurred: {message}", ex.Message);
                await HandleExceptionAsync(context, ex);
            }
        }
        private Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            var statusCode = exception switch
            {
                BadRequestException => StatusCodes.Status400BadRequest,
                ConflictException => StatusCodes.Status409Conflict,
                ForbiddenException => StatusCodes.Status403Forbidden,
                InternalServerErrorException => StatusCodes.Status500InternalServerError,
                NotFoundException => StatusCodes.Status404NotFound,
                UnauthorizedException => StatusCodes.Status401Unauthorized,
                UnprocessableEntityException => StatusCodes.Status422UnprocessableEntity,
                Exception => StatusCodes.Status500InternalServerError
            };


            context.Response.StatusCode = statusCode;
            context.Response.ContentType = "application/json";

            var response = new
            {
                error = statusCode == StatusCodes.Status500InternalServerError ? "Internal server error" : exception.Message,
                status = statusCode
            };
            
            return context.Response.WriteAsJsonAsync(response);
        }
    }
}
