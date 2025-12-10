using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using pr6.Common.CustomExceptions;
using pr6.Context;
using pr6.Interfaces;
using pr6.Models.DTO;

namespace pr6.Services
{
    public class AuthenticationService : IAuthenticationService
    {
        ApplicationContext _context;
        IHashService _hashService;
        IMemoryCache _cache;
        IRandomService _randomService;
        IMailService _mailService;
        ITokenService _tokenService;
        public AuthenticationService(ApplicationContext context,
            IHashService hashService,
            IMemoryCache cache,
            IRandomService randomService,
            IMailService mailService,
            ITokenService tokenService)
        {
            _context = context;
            _hashService = hashService;
            _cache = cache;
            _randomService = randomService;
            _mailService = mailService;
            _tokenService = tokenService;
        }

        public async Task StartAuthenticateAsync(UserCredentialsDTO userCredentials)
        {
            var existsUser = await _context.Users.FirstOrDefaultAsync(u => u.Mail == userCredentials.Mail);
            if (existsUser is null) throw new UnauthorizedException("User with this credentials not found");

            var passwordIsValid = _hashService.Verify(userCredentials.Password, existsUser.PasswordHash);
            if (!passwordIsValid) throw new UnauthorizedException("User with this credentials not found");

            var code = _randomService.GenerateTempCode();
            await _mailService.SendMailAsync(userCredentials.Mail, "Подтверждение авторизации", $"Ваш код для подтверждения авторизации: {code}.");

            _cache.Set("Authenticate_" + userCredentials.Mail, code);
        }
        public async Task<TokenPairDTO> EndAuthenticateAsync(string mail, string verifyCode)
        {
            var isGet = _cache.TryGetValue("Authenticate_" + mail, out string code);
            if (!isGet) throw new ForbiddenException("User with this credentials not found");

            if (code != verifyCode) throw new ForbiddenException("Invalid code");

            var user = _context.Users.Single(u => u.Mail == mail);
            return _tokenService.GetJWTPair(user);
        }
    }
}
