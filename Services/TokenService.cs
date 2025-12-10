using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using pr6.Interfaces;
using pr6.Models;
using pr6.Models.DTO;
using pr6.Models.Options;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace pr6.Services
{
    public class TokenService : ITokenService
    {
        JWTOptions _jwtOptions;
        public TokenService(IOptions<JWTOptions> jwtOptions) 
        {
            _jwtOptions = jwtOptions.Value;
        }
        public TokenPairDTO GetJWTPair(User user) => 
            new TokenPairDTO
            {
                AccessToken = GenerateAccessToken(user),
                RefreshToken = GenerateRefreshToken(user)
            };
        private string GenerateAccessToken(User user)
        {
            List<Claim> claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Mail),
                new Claim("token-type", "access")
            };

            var jwt = new JwtSecurityToken(
                issuer: _jwtOptions.Issuer,
                audience: user.Id.ToString(),
                expires: DateTime.UtcNow.AddMinutes(_jwtOptions.LifeTimeAccessFromMinutes),
                claims: claims,
                signingCredentials: new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.AccessKey)), SecurityAlgorithms.HmacSha256));

            return new JwtSecurityTokenHandler().WriteToken(jwt);
        }
        private string GenerateRefreshToken(User user)
        {
            List<Claim> claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Mail),
                new Claim("token-type", "refresh")
            };

            var jwt = new JwtSecurityToken(
                issuer: _jwtOptions.Issuer,
                audience: user.Id.ToString(),
                expires: DateTime.UtcNow.AddMinutes(_jwtOptions.LifeTimeAccessFromMinutes),
                claims: claims,
                signingCredentials: new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.RefreshKey)), SecurityAlgorithms.HmacSha256));

            return new JwtSecurityTokenHandler().WriteToken(jwt);
        }
    }
}
