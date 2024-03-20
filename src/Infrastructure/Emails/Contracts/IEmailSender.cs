using Infrastructure.Emails.Contracts.Requests;

namespace Infrastructure.Emails.Contracts;

public interface IEmailSender
{
    Task SendAsync(EmailContent email);
}