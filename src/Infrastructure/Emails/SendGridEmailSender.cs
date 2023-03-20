using Domain.Emails.Requests;
using Domain.Emails.Services;
using Domain.Validation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace Infrastructure.Emails;

public class SendGridEmailSender : IEmailSender
{
    private readonly SendGridClient _client;
    private readonly ILogger<SendGridEmailSender> _logger;

    public SendGridEmailSender(
        IConfiguration configuration,
        ILogger<SendGridEmailSender> logger)
    {
        _logger = logger;
        _client = new SendGridClient(configuration["SendGridApiKey"]);
    }

    public async Task SendAsync(EmailContent email)
    {
        email.ThrowIfNull(nameof(email));

        try
        {
            var response = await _client.SendEmailAsync(Message(email));
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("SendGrid returned unsuccessful status code: {ResponseStatusCode}", response.StatusCode);
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Could not send email via SendGrid");
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