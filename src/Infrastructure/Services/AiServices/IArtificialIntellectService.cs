using Domain.Entities.Companies;
using Infrastructure.Ai;
using Infrastructure.Services.AiServices.Reviews;
using Infrastructure.Services.AiServices.Salaries;

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
        SalariesAiBodyReport report,
        string correlationId = null,
        CancellationToken cancellationToken = default);

    Task<AiChatResult> AnalyzeCompanyReviewsWeeklyUpdateAsync(
        CompanyReviewsAiReport report,
        string correlationId = null,
        CancellationToken cancellationToken = default);
}