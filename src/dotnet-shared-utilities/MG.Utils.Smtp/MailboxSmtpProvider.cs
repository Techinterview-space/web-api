using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using MG.Utils.Abstract;
using MG.Utils.Helpers;
using MG.Utils.Validation;

namespace MG.Utils.Smtp
{
    public class MailboxSmtpProvider : IEmail
    {
        private readonly SmtpEmailSettings _emailSettings;

        public MailboxSmtpProvider(SmtpEmailSettings emailSettings)
        {
            _emailSettings = emailSettings;
        }

        public Task SendAsync(string serializedMessage)
        {
            return SendAsync(serializedMessage.DeserializeAs<EmailMessage>());
        }

        public async Task SendAsync(EmailMessage message)
        {
            message.ThrowIfNull(nameof(message));
            message.ThrowIfInvalid();

            var mimeMessage = new MailMessage
            {
                From = new MailAddress(_emailSettings.From.ToString()),
                Subject = message.Subject,
                Body = message.Body,
                IsBodyHtml = true
            };

            foreach (var email in message.To)
            {
                // TODO Maxim: investigate what is 'name' param
                mimeMessage.To.Add(new MailAddress(email));
            }

            using var client = new SmtpClient(_emailSettings.Server.ToString())
            {
                Port = _emailSettings.Port.ToInt(),
                Credentials = new NetworkCredential(
                    _emailSettings.UserName,
                    _emailSettings.Password),
                EnableSsl = true
            };

            await client.SendMailAsync(mimeMessage);
        }
    }
}