using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Ai.ChatGpt;

public class ChatGptProvider : IAiProvider
{
    private readonly ILogger<ChatGptProvider> _logger;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;

    public ChatGptProvider(
        ILogger<ChatGptProvider> logger,
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
        var apiKey = _configuration["OpenAI:ApiKey"];
        var baseUrl = _configuration["OpenAI:BaseUrl"];
        model ??= _configuration["OpenAI:DefaultModel"];

        if (string.IsNullOrEmpty(apiKey) ||
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

        var request = new ChatGptRequest
        {
            Model = model,
            Messages =
            [
                new ChatGptMessage(
                    "system",
                    systemPrompt),

                new ChatGptMessage(
                    "user",
                    input),
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
            else
            {
                _logger.LogInformation(
                    "OpenAI API request succeed. " +
                    "CorrelationId: {CorrelationId}. " +
                    "StatusCode: {StatusCode}, " +
                    "ReasonPhrase: {ReasonPhrase}. " +
                    "Response: {Response}",
                    correlationId,
                    response.StatusCode,
                    response.ReasonPhrase,
                    responseJson);
            }

            var responseDeserialized = JsonSerializer.Deserialize<ChatGptResponse>(responseJson);
            if (responseDeserialized?.Choices == null)
            {
                _logger.LogError(
                    "OpenAI API response deserialization failed. " +
                    "CorrelationId: {CorrelationId}. " +
                    "Response: {Response}",
                    correlationId,
                    responseJson);

                return AiChatResult.Failure(model);
            }

            return AiChatResult.Success(
                responseDeserialized.Choices
                    .Select(x => new AiChoice(x.Message))
                    .ToList(),
                model);
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

            return AiChatResult.Failure(model);
        }
    }
}