using System.Text;
using System.Text.Json;
using Domain.Entities.Companies;
using Domain.Entities.OpenAI;
using Infrastructure.Database;
using Infrastructure.Services.OpenAi.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services.OpenAi;

public class ClaudeService : IArtificialIntellectService
{
    private readonly ILogger<ClaudeService> _logger;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;
    private readonly DatabaseContext _context;

    public ClaudeService(
        ILogger<ClaudeService> logger,
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration,
        DatabaseContext context)
    {
        _logger = logger;
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
        _context = context;
    }

    public async Task<AiChatResult> AnalyzeCompanyAsync(
        Company company,
        string correlationId = null,
        CancellationToken cancellationToken = default)
    {
        if (company == null ||
            !company.HasRelevantReviews())
        {
            throw new InvalidOperationException("Company does not have relevant reviews.");
        }

        var promptData = await _context
            .OpenAiPrompts
            .FirstOrDefaultAsync(x => x.Id == OpenAiPromptType.Company, cancellationToken);

        var input = JsonSerializer.Serialize(
            new CompanyAnalyzeAiRequest(company));

        return await AnalyzeChatAsync(
            input,
            promptData?.Prompt ?? OpenAiPrompt.DefaultCompanyAnalyzePrompt,
            "claude-3-5-sonnet-20241022",
            correlationId,
            cancellationToken);
    }

    public Task<AiChatResult> AnalyzeChatAsync(
        string input,
        string correlationId = null,
        CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    private async Task<AiChatResult> AnalyzeChatAsync(
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
            string.IsNullOrEmpty(baseUrl))
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
                    new ChatMessage(
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

            var responseDeserialized = JsonSerializer.Deserialize<ChatResponse>(responseJson);
            if (responseDeserialized?.Choices == null)
            {
                _logger.LogError(
                    "Claude API response deserialization failed. " +
                    "CorrelationId: {CorrelationId}. " +
                    "Response: {Response}",
                    correlationId,
                    responseJson);

                return AiChatResult.Failure(model);
            }

            return AiChatResult.Success(responseDeserialized.Choices, model);
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