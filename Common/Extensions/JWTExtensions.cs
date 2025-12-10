using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using pr6.Context;
using pr6.Models.Options;
using System.Text;

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
                    .RequireClaim("token_type", "access")
                    .Build();

                // Политика для Refresh Token
                options.AddPolicy("RefreshTokenOnly", policy =>
                {
                    policy.AuthenticationSchemes.Add("RefreshToken");
                    policy.RequireAuthenticatedUser();
                    policy.RequireClaim("token_type", "refresh");
                });

                // Политика для Access Token (явная, если нужно)
                options.AddPolicy("AccessTokenOnly", policy =>
                {
                    policy.AuthenticationSchemes.Add("AccessToken");
                    policy.RequireAuthenticatedUser();
                    policy.RequireClaim("token_type", "access");
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
                    var tokenType = context.Principal?.FindFirst("token_type")?.Value;
                    if (tokenType != "refresh")
                    {
                        context.Fail("Invalid token type. Refresh token required.");
                    }
                    return Task.CompletedTask;
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
