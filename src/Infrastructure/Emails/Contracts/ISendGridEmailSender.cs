using Infrastructure.Emails.Contracts.Requests;

namespace Infrastructure.Emails.Contracts;

public interface ISendGridEmailSender
{
    Task SendAsync(
        EmailContent email,
        CancellationToken cancellationToken);
}