using System;
using System.Linq;
using System.Threading.Tasks;
using EmailService.Integration.Core.Models;
using MG.Utils.Abstract;
using MG.Utils.Abstract.NonNullableObjects;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace EmailService.Integration.Core.Clients;

public class SendGridEmailSender : IEmailSender
{
    private readonly SendGridClient _client;
    private readonly ILogger<SendGridEmailSender> _logger;

    public SendGridEmailSender(
        IConfiguration configuration,
        ILogger<SendGridEmailSender> logger)
    {
        _logger = logger;
        _client = new SendGridClient(new NonNullableString(configuration["SendGridApiKey"]).ToString());
    }

    public async Task SendAsync(EmailContent email)
    {
        email.ThrowIfNull(nameof(email));

        try
        {
            var response = await _client.SendEmailAsync(Message(email));
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError($"SendGrid returned unsuccessful status code: {response.StatusCode}");
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Could not send email via SendGrid");
        }
    }

    private SendGridMessage Message(EmailContent email)
    {
        SendGridMessage msg = MailHelper.CreateSingleEmailToMultipleRecipients(
            @from: new EmailAddress(email.From),
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