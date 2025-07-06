using Domain.Entities.Companies;
using Infrastructure.Ai;
using Infrastructure.Services.AiServices.Custom.Models;

namespace Infrastructure.Services.AiServices;

public interface IArtificialIntellectService
{
    Task<AiChatResult> AnalyzeCompanyAsync(
        Company company,
        string correlationId = null,
        CancellationToken cancellationToken = default);

    Task<AiChatResult> AnalyzeChatAsync(
        string input,
        string correlationId = null,
        CancellationToken cancellationToken = default);

    Task<AiChatResult> AnalyzeSalariesWeeklyUpdateAsync(
        OpenAiBodyReport report,
        string correlationId = null,
        CancellationToken cancellationToken = default);
}