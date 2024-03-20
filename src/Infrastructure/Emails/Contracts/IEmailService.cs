using Infrastructure.Emails.Contracts.Requests;

namespace Infrastructure.Emails.Contracts;

public interface IEmailService
{
    EmailContent Prepare(EmailSendRequest emailContent);
}