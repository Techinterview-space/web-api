using Domain.Entities.Companies;
using Infrastructure.Services.OpenAi.Models;

namespace Infrastructure.Services.OpenAi;

public interface IOpenAiService
{
    Task<OpenAiChatResult> AnalyzeCompanyAsync(
        Company company,
        string correlationId = null,
        CancellationToken cancellationToken = default);

    Task<OpenAiChatResult> AnalyzeChatAsync(
        string input,
        string correlationId = null,
        CancellationToken cancellationToken = default);
}