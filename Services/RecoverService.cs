using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using pr6.Common.CustomExceptions;
using pr6.Context;
using pr6.Interfaces;

namespace pr6.Services
{
    public class RecoverService : IRecoverService
    {
        IRandomService _randomService;
        IMemoryCache _memoryCache;
        IMailService _mailService;
        IHashService _hashService;
        ApplicationContext _context;
        public RecoverService(IRandomService randomService,
            IMemoryCache memoryCache,
            IMailService mailService,
            IHashService hashService,
            ApplicationContext context) 
        {
            _randomService = randomService;
            _memoryCache = memoryCache;
            _mailService = mailService;
            _hashService = hashService;
            _context = context;
        }
        public async Task StartRecoverPasswordAsync(string mail, CancellationToken cancellationToken)
        {
            var tempCode = _randomService.GenerateTempCode();
            _memoryCache.Set("RecoverPassword_" + mail, tempCode);
            await _mailService.SendMailAsync(mail, "Восстановление пароля", $"Ваш код для восстановления пароля: {tempCode}", cancellationToken);
        }
        public async Task EndRecoverPasswordAsync(string mail, string code, string newPassword, CancellationToken cancellationToken)
        {
            var isGet = _memoryCache.TryGetValue("RecoverPassword_" + mail, out string tempCode);
            if (!isGet) throw new ForbiddenException("Invalid mail address");

            if (tempCode != code) throw new ForbiddenException("Invalid code");

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Mail == mail, cancellationToken);
            if (user is null) throw new ForbiddenException("Invalid mail address");
            user.PasswordHash = _hashService.Hash(newPassword);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
