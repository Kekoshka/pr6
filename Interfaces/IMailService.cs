using System.Threading;

namespace pr6.Interfaces
{
    public interface IMailService
    {
        Task SendMailAsync(string email, string subject, string message, CancellationToken cancellationToken);
    }
}
