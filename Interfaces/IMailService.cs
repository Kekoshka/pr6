namespace pr6.Interfaces
{
    public interface IMailService
    {
        Task SendMailAsync(string email, string subject, string message);
    }
}
