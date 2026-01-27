using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Infrastructure.Jwt;
using Infrastructure.Services.AiServices.Salaries;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services.AiServices.Custom;

public class CustomOpenAiService : ICustomOpenAiService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;
    private readonly ILogger<CustomOpenAiService> _logger;

    public CustomOpenAiService(
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration,
        ILogger<CustomOpenAiService> logger)
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
        SalariesAiBodyReport report,
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
            var client = _httpClientFactory.CreateClient();

            client.BaseAddress = new Uri(apiUrl);
            var request = new HttpRequestMessage(HttpMethod.Post, apiUrl);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", GetBearer());
            request.Content = new StringContent(
                JsonSerializer.Serialize(report),
                Encoding.UTF8,
                "application/json");

            var response = await client.SendAsync(request, cancellationToken);

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