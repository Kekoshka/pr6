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
        MailService _mailService;
        public RegistrationService(ApplicationContext context,
            IMemoryCache memoryCache,
            IHashService hashService,
            MailService mailService) 
        {
            _context = context;
            _hashService = hashService;
            _mailService = mailService;
            _memoryCache = memoryCache;
        }
        public async void StartRegistration(UserCredentialsDTO userCredentials, CancellationToken cancellationToken)
        {
            var userIsExists = await _context.Users.AnyAsync(u => u.Mail == userCredentials.Mail);
            if (userIsExists) throw new ConflictException("User with this login already exists");

            string code = GenerateTempCode();
            var hashedPassword = _hashService.Hash(userCredentials.Password);
            User newUser = new()
            {
                Id = Guid.NewGuid(),
                Mail = userCredentials.Mail,
                PasswordHash = hashedPassword,
            };
            _memoryCache.Set(code, newUser);
        }
        public async Task EndRegistration(string code)
        {
            bool isUserExists = _memoryCache.TryGetValue(code, out User newUser);

            if (!isUserExists) throw new NotFoundException("Invalid code"); //ДРУГОЕ ИСКЛЮЧЕНИЕ
            await _context.Users.AddAsync(newUser);
        }
        private string GenerateTempCode() =>
            new Random().Next(100000, 999999).ToString();

    }
}
