using Infrastructure.Emails.Contracts;
using Infrastructure.Emails.Contracts.Requests;
using Mailjet.Client;
using Mailjet.Client.Resources;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;

namespace Infrastructure.Emails;

public class MailjetEmailApiSender : IEmailApiSender
{
    private readonly ILogger<MailjetEmailApiSender> _logger;
    private readonly IConfiguration _configuration;

    public MailjetEmailApiSender(
        ILogger<MailjetEmailApiSender> logger,
        IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
    }

    public async Task SendAsync(
        EmailContent email,
        CancellationToken cancellationToken)
    {
        var apiKeyPublic = _configuration["MailjetApiKey"];
        var apiKeyPrivate = _configuration["MailjetApiSecret"];

        var client = new MailjetClient(apiKeyPublic, apiKeyPrivate);

        var request = new MailjetRequest
            {
                Resource = Send.Resource,
            }
            .Property(
                Send.Messages,
                new JArray
                {
                    new JObject
                    {
                        {
                            "From",
                            new JObject
                            {
                                { "Email", email.From },
                                { "Name", "Techinterview.space" }
                            }
                        },
                        {
                            "To",
                            new JArray
                            {
                                new JObject
                                {
                                    { "Email", email.Recipients.FirstOrDefault() },
                                }
                            }
                        },
                        { "Subject", email.Subject },
                        { "TextPart", "Email from techinterview.space" },
                        { "HTMLPart", email.HtmlBody }
                    }
                });

        var response = await client.PostAsync(request);
        if (response.IsSuccessStatusCode)
        {
            _logger.LogInformation(
                "Email was sent successfully. StatusCode: {StatusCode}. Count: {Count}. Response {Response}",
                response.StatusCode,
                response.GetCount(),
                response.GetData());

            return;
        }

        _logger.LogError(
            "Mailjet returned unsuccessful status code: {StatusCode}. ErrorInfo: {ErrorInfo}. Data: {Data}. ErrorMessage: {ErrorMessage}",
            response.StatusCode,
            response.GetErrorInfo(),
            response.GetData(),
            response.GetErrorMessage());
    }
}