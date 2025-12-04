using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using pr6.Common.CustomExceptions;
using pr6.Context;
using pr6.Interfaces;
using pr6.Models;
using pr6.Models.DTO;

namespace pr6.Services
{
    public class RegistrationService : IRegistrationService
    {
        ApplicationContext _context;
        IMemoryCache _memoryCache;
        IHashService _hashService;
        IMailService _mailService;
        IRandomService _randomService;
        public RegistrationService(ApplicationContext context,
            IMemoryCache memoryCache,
            IHashService hashService,
            IMailService mailService,
            IRandomService randomService) 
        {
            _context = context;
            _hashService = hashService;
            _mailService = mailService;
            _memoryCache = memoryCache;
            _randomService = randomService;
        }
        public async Task StartRegistrationAsync(UserCredentialsDTO userCredentials, CancellationToken cancellationToken)
        {
            var userIsExists = await _context.Users.AnyAsync(u => u.Mail == userCredentials.Mail);
            if (userIsExists) throw new ConflictException("User with this login already exists");

            string code = _randomService.GenerateTempCode();
            var hashedPassword = _hashService.Hash(userCredentials.Password);
            User newUser = new()
            {
                Id = Guid.NewGuid(),
                Mail = userCredentials.Mail,
                PasswordHash = hashedPassword,
            };
            _memoryCache.Set(code, newUser);
            await _mailService.SendMailAsync(userCredentials.Mail, "Код для подтверждения регистрации", $"Ваш код для подтверждения регистрации: {code}.");
            
        }
        public async Task EndRegistrationAsync(string code,CancellationToken cancellationToken)
        {
            bool isUserExists = _memoryCache.TryGetValue(code, out User newUser);
            if (!isUserExists) throw new ForbiddenException("Invalid code");

            await _context.Users.AddAsync(newUser);
        }


    }
}
