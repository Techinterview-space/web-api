using System;
using System.Threading.Tasks;
using EmailService.Integration.Core.Models;
using Microsoft.Extensions.Logging;

namespace EmailService.Integration.Core.Clients;

public class LocalEmailSender : IEmailSender
{
    private readonly ILogger<LocalEmailSender> _logger;

    public LocalEmailSender(ILogger<LocalEmailSender> logger)
    {
        _logger = logger;
    }

    public Task SendAsync(EmailContent email)
    {
        var breaker = Environment.NewLine;

        _logger.LogInformation(
            $"Email was sent{breaker}" +
            $"Subject: {email.Subject}{breaker}" +
            $"Recipients: {email.Recipients.Count}{breaker}" +
            $"Cc: {email.Cc.Count}{breaker}" +
            $"Bcc: {email.HiddenCc.Count}{breaker}");

        return Task.CompletedTask;
    }
}