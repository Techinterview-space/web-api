using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Domain.Entities.Companies;
using Infrastructure.Services.OpenAi.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services.OpenAi;

public class OpenAiService : IOpenAiService
{
    private readonly ILogger<OpenAiService> _logger;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;

    public OpenAiService(
        ILogger<OpenAiService> logger,
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration)
    {
        _logger = logger;
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
    }

    public Task<OpenAiChatResult> AnalyzeCompanyAsync(
        Company company,
        string correlationId = null,
        CancellationToken cancellationToken = default)
    {
        if (company == null ||
            !company.HasRelevantReviews())
        {
            throw new InvalidOperationException("Company does not have relevant reviews.");
        }

        const string systemPrompt =
            "You are a helpful career assistant. " +
            "Analyze the company's reviews and provide " +
            "a summary with advice what should user pay more attention on. " +
            "In the request there will be a company total rating, rating history and reviews presented in JSON format.";

        var input = JsonSerializer.Serialize(
            new CompanyAnalyzeRequest(company));

        return AnalyzeChatAsync(
            input,
            systemPrompt,
            correlationId,
            cancellationToken);
    }

    public Task<OpenAiChatResult> AnalyzeChatAsync(
        string input,
        string correlationId = null,
        CancellationToken cancellationToken = default)
    {
        return AnalyzeChatAsync(
            input,
            "You are a helpful assistant. Analyze the user's input and provide a response.",
            correlationId,
            cancellationToken);
    }

    private async Task<OpenAiChatResult> AnalyzeChatAsync(
        string input,
        string systemPrompt,
        string correlationId = null,
        CancellationToken cancellationToken = default)
    {
        var apiKey = _configuration["OpenAI:ApiKey"];
        var model = _configuration["OpenAI:Model"];
        var baseUrl = _configuration["OpenAI:BaseUrl"];

        if (string.IsNullOrEmpty(apiKey) ||
            string.IsNullOrEmpty(model) ||
            string.IsNullOrEmpty(baseUrl))
        {
            _logger.LogError(
                "OpenAI configuration is missing. " +
                "CorrelationId: {CorrelationId}. " +
                "ApiKey: {ApiKey}, " +
                "Model: {Model}, " +
                "BaseUrl: {BaseUrl}",
                correlationId,
                apiKey?.Length.ToString() ?? "-",
                model,
                baseUrl);

            throw new InvalidOperationException("OpenAI configuration is not set.");
        }

        var request = new ChatRequest
        {
            Model = model,
            Messages =
            [
                new ChatMessage
                {
                    Role = "system",
                    Content = systemPrompt,
                },

                new ChatMessage
                {
                    Role = "user",
                    Content = input
                },
            ]
        };

        var requestJson = JsonSerializer.Serialize(request);
        var httpRequest = new HttpRequestMessage(
            HttpMethod.Post,
            $"{baseUrl}chat/completions");

        httpRequest.Headers.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            apiKey);

        httpRequest.Content = new StringContent(
            requestJson,
            Encoding.UTF8,
            "application/json");

        var client = _httpClientFactory.CreateClient("OpenAI");

        try
        {
            var response = await client.SendAsync(httpRequest, cancellationToken);
            var responseJson = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError(
                    "OpenAI API request failed. " +
                    "CorrelationId: {CorrelationId}. " +
                    "StatusCode: {StatusCode}, " +
                    "ReasonPhrase: {ReasonPhrase}. " +
                    "Response: {Response}",
                    correlationId,
                    response.StatusCode,
                    response.ReasonPhrase,
                    responseJson);
            }

            var responseDeserialized = JsonSerializer.Deserialize<ChatResponse>(responseJson);
            if (responseDeserialized?.Choices == null)
            {
                _logger.LogError(
                    "OpenAI API response deserialization failed. " +
                    "CorrelationId: {CorrelationId}. " +
                    "Response: {Response}",
                    correlationId,
                    responseJson);

                return OpenAiChatResult.Failure();
            }

            return OpenAiChatResult.Success(responseDeserialized.Choices);
        }
        catch (Exception e)
        {
            _logger.LogError(
                e,
                "An error occurred while calling OpenAI API. " +
                "CorrelationId: {CorrelationId}. " +
                "Input: {Input}",
                correlationId,
                input);

            return OpenAiChatResult.Failure();
        }
    }
}