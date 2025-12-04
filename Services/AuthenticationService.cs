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
        public AuthenticationService(ApplicationContext context,
            IHashService hashService,
            IMemoryCache cache)
        {
            _context = context;
            _hashService = hashService;
            _cache = cache;
        }

        public async Task Authenticate(UserCredentialsDTO userCredentials)
        {
            var existsUser = await _context.Users.FirstOrDefaultAsync(u => u.Mail == userCredentials.Mail);
            if (existsUser is null) throw new UnauthorizedException("User with this credentials not found");

            var passwordIsValid = _hashService.Verify(userCredentials.Password, existsUser.PasswordHash);
            if (!passwordIsValid) throw new UnauthorizedException("User with this credentials not found");

            _cache.Set("login",)
        }
    }
}
