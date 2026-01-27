using Infrastructure.Emails.Contracts;
using Infrastructure.Emails.Contracts.Requests;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Emails;

public class LocalEmailApiSender : IEmailApiSender
{
    private readonly ILogger<LocalEmailApiSender> _logger;

    public LocalEmailApiSender(ILogger<LocalEmailApiSender> logger)
    {
        _logger = logger;
    }

    public Task SendAsync(
        EmailContent email,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Email was sent. Subject: {Subject}. Recipients: {RecipientsCount}",
            email.Subject,
            email.Recipients.Count);

        return Task.CompletedTask;
    }
}