﻿using Domain.Validation;
using Infrastructure.Emails.Contracts;
using Infrastructure.Emails.Contracts.Requests;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace Infrastructure.Emails;

public class SendgridEmailApiSender : IEmailApiSender
{
    private readonly SendGridClient _client;
    private readonly ILogger<SendgridEmailApiSender> _logger;

    public SendgridEmailApiSender(
        IConfiguration configuration,
        ILogger<SendgridEmailApiSender> logger)
    {
        _logger = logger;
        _client = new SendGridClient(configuration["SendGridApiKey"]);
    }

    public async Task SendAsync(
        EmailContent email,
        CancellationToken cancellationToken)
    {
        email.ThrowIfNull(nameof(email));

        try
        {
            var response = await _client.SendEmailAsync(Message(email), cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError(
                    "SendGrid returned unsuccessful status code: {ResponseStatusCode}",
                    response.StatusCode);

                return;
            }

            var responseString = await response.Body.ReadAsStringAsync(cancellationToken);
            _logger.LogInformation(
                "Email was sent successfully. StatusCode: {StatusCode}. Response: {Response}",
                response.StatusCode,
                responseString);
        }
        catch (Exception e)
        {
            _logger.LogError(
                e,
                "Could not send email via SendGrid");
        }
    }

    private SendGridMessage Message(
        EmailContent email)
    {
        var msg = MailHelper.CreateSingleEmailToMultipleRecipients(
            from: new EmailAddress(email.From),
            tos: email.Recipients.Select(x => new EmailAddress(x)).ToList(),
            subject: email.Subject,
            plainTextContent: string.Empty,
            htmlContent: email.HtmlBody);

        if (email.Cc.Any())
        {
            msg.AddCcs(email.Cc.Select(x => new EmailAddress(x)).ToList());
        }

        if (email.HiddenCc.Any())
        {
            msg.AddBccs(email.HiddenCc.Select(x => new EmailAddress(x)).ToList());
        }

        return msg;
    }
}