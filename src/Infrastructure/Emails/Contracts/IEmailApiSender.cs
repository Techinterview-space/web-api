using Infrastructure.Emails.Contracts.Requests;

namespace Infrastructure.Emails.Contracts;

public interface IEmailApiSender
{
    Task SendAsync(
        EmailContent email,
        CancellationToken cancellationToken);
}