using Microsoft.Extensions.Options;
using pr6.Context;
using pr6.Models.Options;

namespace pr6.Common.Extensions
{
    public static class JWTExtensions
    {
        public static void ConfigureJWTAuthentication(IServiceCollection services)
        {
            var serviceProvider = services.BuildServiceProvider();
            JWTOptions JWToptions = serviceProvider.GetService<IOptions<JWTOptions>>().Value;

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                options.TokenValidationParameters = new TokenValidationParameters
                {

                    ValidateIssuer = true,
                    ValidIssuer = JWToptions.Issuer,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(JWToptions.Key)),
                    AudienceValidator = (audiences, securityToken, validationParameters) =>
                        ValidateAudience(services, audiences)
                });
        }
        private static bool ValidateAudience(IServiceCollection services, IEnumerable<string> audiences)
        {
            using var scope = services.BuildServiceProvider().CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationContext>();
            return dbContext.Users.Any(u => u.Id.ToString() == audiences.FirstOrDefault());
        }
    }
}
