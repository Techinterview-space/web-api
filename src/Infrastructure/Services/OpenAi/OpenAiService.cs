using System.Net.Http.Headers;
using Infrastructure.Jwt;
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

            responseContent = await client.GetStringAsync(string.Empty, cancellationToken);
            return responseContent;
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