using System.Net;
using System.Net.Mail;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;
using pr6.Common.Options;
using pr6.Interfaces;
using pr6.Models.Options;
using SmtpClient = MailKit.Net.Smtp.SmtpClient;

namespace pr6.Services
{
    public class MailService : IMailService
    {
        MailOptions _mailOptions { get; set; }

        public MailService(IOptions<MailOptions> mailOptions) 
        {
            _mailOptions = mailOptions.Value;
        }

        public async Task SendMailAsync(string email, string subject, string message)
        {
            var mailMessage = new MimeMessage();

            mailMessage.From.Add(new MailboxAddress(_mailOptions.FromName, _mailOptions.FromAddress));
            mailMessage.To.Add(new MailboxAddress("", email));
            mailMessage.Subject = subject;
            mailMessage.Body = new TextPart(MimeKit.Text.TextFormat.Html) { Text = message };

            using (var client = new SmtpClient())
            {
                client.LocalDomain = _mailOptions.LocalDomain;

                await client.ConnectAsync(_mailOptions.MailServerAddress, Convert.ToInt32(_mailOptions.MailServerPort), SecureSocketOptions.Auto).ConfigureAwait(false);
                await client.AuthenticateAsync(new NetworkCredential(_mailOptions.UserId, _mailOptions.UserPassword));
                await client.SendAsync(mailMessage).ConfigureAwait(false);
                await client.DisconnectAsync(true).ConfigureAwait(false);
            }
        }
    }
}
