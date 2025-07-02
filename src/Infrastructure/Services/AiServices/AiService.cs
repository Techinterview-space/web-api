using System.Text.Json;
using Domain.Entities.Companies;
using Domain.Entities.OpenAI;
using Infrastructure.Ai;
using Infrastructure.Database;
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

        var promptData = await _context
            .OpenAiPrompts
            .OrderByDescending(x => x.CreatedAt)
            .FirstOrDefaultAsync(
                x => x.Type == OpenAiPromptType.Company && x.IsActive,
                cancellationToken)
            ?? throw new InvalidOperationException($"System does not have eny prompts for {OpenAiPromptType.Company}.");

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
        var promptData = await _context
            .OpenAiPrompts
            .OrderByDescending(x => x.CreatedAt)
            .FirstOrDefaultAsync(
                x => x.Type == OpenAiPromptType.Chat && x.IsActive,
                cancellationToken)
                     ?? throw new InvalidOperationException($"System does not have eny prompts for {OpenAiPromptType.Chat}.");

        var aiProvider = _aiProviderFactory.GetProvider(promptData.Engine);

        return await aiProvider.AnalyzeChatAsync(
            input,
            promptData.Prompt,
            promptData.Model,
            correlationId,
            cancellationToken);
    }
}