using Domain.Emails.Requests;

namespace Domain.Emails.Services;

public interface IEmailService
{
    EmailContent Prepare(EmailSendRequest emailContent);
}