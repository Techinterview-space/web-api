using Domain.Entities.Companies;
using Infrastructure.Ai;

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
}