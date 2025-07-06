using System.Text.Json;
using Domain.Entities.Companies;
using Domain.Entities.Prompts;
using Infrastructure.Ai;
using Infrastructure.Database;
using Infrastructure.Services.AiServices.Custom.Models;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services.AiServices;

public class AiService : IArtificialIntellectService
{
    private readonly DatabaseContext _context;
    private readonly IAiProviderFactory _aiProviderFactory;

    public AiService(
        DatabaseContext context,
        IAiProviderFactory aiProviderFactory)
    {
        _context = context;
        _aiProviderFactory = aiProviderFactory;
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

        var promptData = await GetActivePromptAsync(OpenAiPromptType.Company, cancellationToken);

        var input = JsonSerializer.Serialize(
            new CompanyAnalyzeAiRequest(company));

        var aiProvider = _aiProviderFactory.GetProvider(promptData.Engine);

        return await aiProvider.AnalyzeChatAsync(
            input,
            promptData.Prompt,
            promptData.Model,
            correlationId,
            cancellationToken);
    }

    public async Task<AiChatResult> AnalyzeChatAsync(
        string input,
        string correlationId = null,
        CancellationToken cancellationToken = default)
    {
        var promptData = await GetActivePromptAsync(OpenAiPromptType.Chat, cancellationToken);
        var aiProvider = _aiProviderFactory.GetProvider(promptData.Engine);

        return await aiProvider.AnalyzeChatAsync(
            input,
            promptData.Prompt,
            promptData.Model,
            correlationId,
            cancellationToken);
    }

    public async Task<AiChatResult> AnalyzeSalariesWeeklyUpdateAsync(
        OpenAiBodyReport report,
        string correlationId = null,
        CancellationToken cancellationToken = default)
    {
        var promptData = await GetActivePromptAsync(OpenAiPromptType.SalariesWeeklyUpdate, cancellationToken);
        var aiProvider = _aiProviderFactory.GetProvider(promptData.Engine);

        return await aiProvider.AnalyzeChatAsync(
            report.ToJson(),
            promptData.Prompt,
            promptData.Model,
            correlationId,
            cancellationToken);
    }

    private async Task<OpenAiPrompt> GetActivePromptAsync(
        OpenAiPromptType promptType,
        CancellationToken cancellationToken)
    {
        return await _context
            .OpenAiPrompts
            .OrderByDescending(x => x.CreatedAt)
            .FirstOrDefaultAsync(
                x => x.Type == promptType && x.IsActive,
                cancellationToken)
            ?? throw new InvalidOperationException($"System does not have any prompts for {promptType}.");
    }
}