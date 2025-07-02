using System.Net.Http.Headers;
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

public class ChatGptService : IArtificialIntellectService
{
    private readonly ILogger<ChatGptService> _logger;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;
    private readonly DatabaseContext _context;

    public ChatGptService(
        ILogger<ChatGptService> logger,
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
            promptData?.Model,
            correlationId,
            cancellationToken);
    }

    public async Task<AiChatResult> AnalyzeChatAsync(
        string input,
        string correlationId = null,
        CancellationToken cancellationToken = default)
    {
        var prompt = await _context
                         .OpenAiPrompts
                         .FirstOrDefaultAsync(x => x.Id == OpenAiPromptType.Chat, cancellationToken);

        return await AnalyzeChatAsync(
            input,
            prompt?.Prompt ?? OpenAiPrompt.DefaultChatAnalyzePrompt,
            prompt?.Model,
            correlationId,
            cancellationToken);
    }

    private async Task<AiChatResult> AnalyzeChatAsync(
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

            var responseDeserialized = JsonSerializer.Deserialize<ChatResponse>(responseJson);
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

            return AiChatResult.Success(responseDeserialized.Choices, model);
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