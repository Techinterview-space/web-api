using Domain.Validation;
using Infrastructure.Emails.Contracts;
using Infrastructure.Emails.Contracts.Requests;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Resend;

namespace Infrastructure.Emails;

public class ResendEmailApiSender : IEmailApiSender
{
    private readonly IResend _client;
    private readonly ILogger<ResendEmailApiSender> _logger;

    public ResendEmailApiSender(
        IConfiguration configuration,
        ILogger<ResendEmailApiSender> logger)
    {
        _logger = logger;
        var apiKey = configuration["ResendComApiKey"]
            ?? throw new InvalidOperationException("ResendComApiKey configuration is missing");

        _client = ResendClient.Create(apiKey);
    }

    public async Task SendAsync(
        EmailContent email,
        CancellationToken cancellationToken)
    {
        email.ThrowIfNull(nameof(email));

        try
        {
            var message = new EmailMessage
            {
                From = email.From,
                Subject = email.Subject,
                HtmlBody = email.HtmlBody
            };

            foreach (var recipient in email.Recipients)
            {
                message.To.Add(recipient);
            }

            if (email.Cc.Any())
            {
                message.Cc ??= new EmailAddressList();
                foreach (var cc in email.Cc)
                {
                    message.Cc.Add(cc);
                }
            }

            if (email.HiddenCc.Any())
            {
                message.Bcc ??= new EmailAddressList();
                foreach (var bcc in email.HiddenCc)
                {
                    message.Bcc.Add(bcc);
                }
            }

            var response = await _client.EmailSendAsync(message, cancellationToken);

            _logger.LogInformation(
                "Email was sent successfully via Resend. MessageId: {MessageId}",
                response.Content);
        }
        catch (Exception e)
        {
            _logger.LogError(
                e,
                "Could not send email via Resend");
        }
    }
}
