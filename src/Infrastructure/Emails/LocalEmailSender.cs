using Infrastructure.Emails.Contracts;
using Infrastructure.Emails.Contracts.Requests;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Emails;

public class LocalEmailSender : ISendGridEmailSender
{
    private readonly ILogger<LocalEmailSender> _logger;

    public LocalEmailSender(ILogger<LocalEmailSender> logger)
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