using Domain.Entities.Companies;
using Infrastructure.Services.OpenAi.Models;

namespace Infrastructure.Services.OpenAi;

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