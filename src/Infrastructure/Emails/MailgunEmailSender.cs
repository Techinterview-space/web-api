using System.Net;
using Infrastructure.Emails.Contracts;
using Infrastructure.Emails.Contracts.Requests;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Emails;

public class MailgunEmailSender : IEmailApiSender
{
    private readonly ILogger<MailgunEmailSender> _logger;
    private readonly HttpClient _httpClient;

    public MailgunEmailSender(
        ILogger<MailgunEmailSender> logger,
        HttpClient httpClient)
    {
        _logger = logger;
        _httpClient = httpClient;
    }

    public async Task SendAsync(
        EmailContent email,
        CancellationToken cancellationToken)
    {
        var form = new Dictionary<string, string>
        {
            ["from"] = email.From,
            ["to"] = email.Recipients.FirstOrDefault(),
            ["subject"] = email.Subject,
            ["html"] = email.HtmlBody
        };

        var response = await _httpClient.PostAsync(
            string.Empty,
            new FormUrlEncodedContent(form),
            cancellationToken);

        if (response.StatusCode == HttpStatusCode.OK)
        {
            return;
        }

        _logger.LogError(
            "Mailgun returned unsuccessful status code: {ResponseStatusCode}. Reason: {ReasonPhrase}",
            response.StatusCode,
            response.ReasonPhrase);
    }
}