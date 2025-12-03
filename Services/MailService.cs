using pr6.Interfaces;

namespace pr6.Services
{
    public class MailService : IMailService
    {




        private string GenerateTempCode() => 
            new Random().Next(100000, 999999).ToString();
    }
}
