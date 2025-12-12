using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using pr6.Context;
using pr6.Models.Options;
using System.Text;
using System.Text.Json;

namespace pr6.Common.Extensions
{
    public static class JWTExtensions
    {
        public static void ConfigureJWTAuthentication(this IServiceCollection services, IConfiguration configuration)
        {
            var jwtOptions = configuration.GetSection(nameof(JWTOptions)).Get<JWTOptions>();

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    ConfigureAccessTokenValidation(options, jwtOptions, services);
                })
                .AddJwtBearer("RefreshToken", options =>
                {
                    ConfigureRefreshTokenValidation(options, jwtOptions, services);
                });

            services.AddAuthorization(options =>
            {
                // Политика по умолчанию - требует Access Token
                options.DefaultPolicy = new AuthorizationPolicyBuilder("AccessToken")
                    .RequireAuthenticatedUser()
                    .RequireClaim("token-type", "access")
                    .Build();

                // Политика для Refresh Token
                options.AddPolicy("RefreshTokenOnly", policy =>
                {
                    policy.AuthenticationSchemes.Add("RefreshToken");
                    policy.RequireAuthenticatedUser();
                    policy.RequireClaim("token-type", "refresh");
                });

                // Политика для Access Token (явная, если нужно)
                options.AddPolicy("AccessTokenOnly", policy =>
                {
                    policy.AuthenticationSchemes.Add("AccessToken");
                    policy.RequireAuthenticatedUser();
                    policy.RequireClaim("token-type", "access");
                });
            });
        }
        private static bool ValidateAudience(IServiceCollection services, IEnumerable<string> audiences)
        {
            using var scope = services.BuildServiceProvider().CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationContext>();
            return dbContext.Users.Any(u => u.Id.ToString() == audiences.FirstOrDefault());
        }
        private static void ConfigureRefreshTokenValidation(JwtBearerOptions options, JWTOptions jwtOptions,IServiceCollection services)
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {

                ValidateIssuer = true,
                ValidIssuer = jwtOptions.Issuer,
                ValidateAudience = true,
                ValidateLifetime = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.RefreshKey)),
                AudienceValidator = (audiences, securityToken, validationParameters) =>
                    ValidateAudience(services, audiences)
            };
            options.Events = new JwtBearerEvents
            {
                OnTokenValidated = context =>
                {
                    var tokenType = context.Principal?.FindFirst("token-type")?.Value;
                    if (tokenType != "refresh")
                    {
                        context.Fail("Invalid token type. Refresh token required.");
                    }
                    return Task.CompletedTask;
                },
                OnForbidden = context =>
                {
                    var response = context.HttpContext.Response;
                    response.StatusCode = 403;
                    response.ContentType = "application/json";

                    // Можно добавить дополнительную информацию
                    var result = new
                    {
                        error = "forbidden",
                        error_description = "Access to this resource is forbidden",
                        details = "Token validation failed or insufficient permissions",
                        timestamp = DateTime.UtcNow
                    };

                    return response.WriteAsync(JsonSerializer.Serialize(result));
                },
                OnAuthenticationFailed = context =>
                {

                    // Определяем тип ошибки
                    string error;
                    string description;

                    if (context.Exception is SecurityTokenExpiredException)
                    {
                        error = "token_expired";
                        description = "The token has expired";
                    }
                    else if (context.Exception is SecurityTokenInvalidSignatureException)
                    {
                        error = "invalid_signature";
                        description = "Token signature is invalid";
                    }
                    else if (context.Exception is SecurityTokenInvalidIssuerException)
                    {
                        error = "invalid_issuer";
                        description = "Token issuer is invalid";
                    }
                    else if (context.Exception is SecurityTokenInvalidAudienceException)
                    {
                        error = "invalid_audience";
                        description = "Token audience is invalid";
                    }
                    else
                    {
                        error = "invalid_token";
                        description = "Token validation failed";
                    }

                    context.Response.StatusCode = 403;
                    context.Response.ContentType = "application/json";

                    var result = new
                    {
                        error,
                        error_description = description,
                        details = context.Exception.Message,
                        timestamp = DateTime.UtcNow
                    };

                    return context.Response.WriteAsync(JsonSerializer.Serialize(result));
                }
            };
                
        }
        private static void ConfigureAccessTokenValidation(JwtBearerOptions options, JWTOptions jwtOptions, IServiceCollection services)
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {

                ValidateIssuer = true,
                ValidIssuer = jwtOptions.Issuer,
                ValidateAudience = true,
                ValidateLifetime = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.AccessKey)),
                AudienceValidator = (audiences, securityToken, validationParameters) =>
                    ValidateAudience(services, audiences)
            };
            options.Events = new JwtBearerEvents
            {
                OnTokenValidated = context =>
                {
                    // Дополнительная проверка, что это access token
                    var tokenType = context.Principal?.FindFirst("token_type")?.Value;
                    if (tokenType != "access")
                    {
                        context.Fail("Invalid token type. Access token required.");
                    }
                    return Task.CompletedTask;
                }
            };
        }
    }
}
