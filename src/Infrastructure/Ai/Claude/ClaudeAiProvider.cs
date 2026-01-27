using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Ai.Claude;

public class ClaudeAiProvider : IAiProvider
{
    private const string ApiKeyTemplate = "__CLAUDE_API_KEY";

    private readonly ILogger<ClaudeAiProvider> _logger;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;

    public ClaudeAiProvider(
        ILogger<ClaudeAiProvider> logger,
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration)
    {
        _logger = logger;
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
    }

    public async Task<AiChatResult> AnalyzeChatAsync(
        string input,
        string systemPrompt,
        string model = null,
        string correlationId = null,
        CancellationToken cancellationToken = default)
    {
        var apiKey = _configuration["Claude:ApiKey"];
        var baseUrl = _configuration["Claude:BaseUrl"];
        model ??= _configuration["Claude:DefaultModel"];

        if (string.IsNullOrEmpty(apiKey) ||
            string.IsNullOrEmpty(baseUrl) ||
            apiKey == ApiKeyTemplate)
        {
            _logger.LogError(
                "Claude configuration is missing. " +
                "CorrelationId: {CorrelationId}. " +
                "ApiKey: {ApiKey}, " +
                "Model: {Model}, " +
                "BaseUrl: {BaseUrl}",
                correlationId,
                apiKey?.Length.ToString() ?? "-",
                model,
                baseUrl);

            throw new InvalidOperationException("Claude configuration is not set.");
        }

        var requestJson = JsonSerializer.Serialize(
            new ClaudeRequest
            {
                Model = model,
                SystemPrompt = systemPrompt,
                MaxTokens = 2048,
                Messages =
                [
                    new ClaudeChatMessage(
                        "user",
                        input),
                ]
            });

        var httpRequest = new HttpRequestMessage(
            HttpMethod.Post,
            baseUrl);

        httpRequest.Headers.Add("x-api-key", apiKey);
        httpRequest.Headers.Add("anthropic-version", "2023-06-01");

        httpRequest.Content = new StringContent(
            requestJson,
            Encoding.UTF8,
            "application/json");

        var client = _httpClientFactory.CreateClient("Claude");

        try
        {
            var response = await client.SendAsync(httpRequest, cancellationToken);
            var responseJson = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError(
                    "Claude API request failed. " +
                    "CorrelationId: {CorrelationId}. " +
                    "StatusCode: {StatusCode}, " +
                    "ReasonPhrase: {ReasonPhrase}. " +
                    "Response: {Response}",
                    correlationId,
                    response.StatusCode,
                    response.ReasonPhrase,
                    responseJson);
            }
            else
            {
                _logger.LogInformation(
                    "Claude API request succeed. " +
                    "CorrelationId: {CorrelationId}. " +
                    "StatusCode: {StatusCode}, " +
                    "ReasonPhrase: {ReasonPhrase}. " +
                    "Response: {Response}",
                    correlationId,
                    response.StatusCode,
                    response.ReasonPhrase,
                    responseJson);
            }

            var responseDeserialized = JsonSerializer.Deserialize<ClaudeChatResponse>(responseJson);
            if (responseDeserialized?.Content == null)
            {
                _logger.LogError(
                    "Claude API response deserialization failed. " +
                    "CorrelationId: {CorrelationId}. " +
                    "Response: {Response}",
                    correlationId,
                    responseJson);

                return AiChatResult.Failure(model);
            }

            return AiChatResult.Success(
                responseDeserialized.Content
                    .Select(x => new AiChoice(
                        responseDeserialized.Model,
                        x))
                    .ToList(),
                model);
        }
        catch (Exception e)
        {
            _logger.LogError(
                e,
                "An error occurred while calling Claude API. " +
                "CorrelationId: {CorrelationId}. " +
                "Input: {Input}",
                correlationId,
                input);

            return AiChatResult.Failure(model);
        }
    }
}