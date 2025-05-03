using System.Net.Http.Headers;
using System.Net.Http.Json;
using Infrastructure.Jwt;
using Infrastructure.Services.OpenAi.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services.OpenAi;

public class OpenAiService : IOpenAiService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;
    private readonly ILogger<OpenAiService> _logger;

    public OpenAiService(
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration,
        ILogger<OpenAiService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
        _logger = logger;
    }

    public string GetBearer()
    {
        var secret = _configuration["OpenAiApiSecret"];
        return new TechinterviewJwtTokenGenerator(secret).ToString();
    }

    public async Task<string> GetAnalysisAsync(
        OpenAiBodyReport report,
        CancellationToken cancellationToken = default)
    {
        var apiUrl = _configuration["OpenAiApiUrl"];
        if (string.IsNullOrEmpty(apiUrl))
        {
            throw new InvalidOperationException("OpenAI API url is not set");
        }

        var responseContent = string.Empty;
        try
        {
            using var client = _httpClientFactory.CreateClient();

            client.BaseAddress = new Uri(apiUrl);
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", GetBearer());

            var response = await client.SendAsync(
                new HttpRequestMessage(
                    HttpMethod.Post,
                    string.Empty)
                {
                    Content = JsonContent.Create(report),
                },
                cancellationToken);

            responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
            if (response.IsSuccessStatusCode)
            {
                return responseContent;
            }

            _logger.LogError(
                "Failed request to OpenAI {Url}. Status {Status}, Response {Response}",
                apiUrl,
                response.StatusCode,
                responseContent);

            return string.Empty;
        }
        catch (Exception e)
        {
            _logger.LogError(
                e,
                "Error while getting OpenAI response from {Url}. Message {Message}. Response {Response}",
                apiUrl,
                e.Message,
                responseContent);

            return string.Empty;
        }
    }
}