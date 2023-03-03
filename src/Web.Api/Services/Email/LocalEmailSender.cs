﻿using System.Threading.Tasks;
using Domain.Emails.Requests;
using Domain.Emails.Services;
using Microsoft.Extensions.Logging;

namespace TechInterviewer.Services.Email;

public class LocalEmailSender : IEmailSender
{
    private readonly ILogger<LocalEmailSender> _logger;

    public LocalEmailSender(ILogger<LocalEmailSender> logger)
    {
        _logger = logger;
    }

    public Task SendAsync(EmailContent email)
    {
        _logger.LogInformation(
            "Email was sent. Subject: {Subject}. Recipients: {RecipientsCount}",
            email.Subject,
            email.Recipients.Count);

        return Task.CompletedTask;
    }
}