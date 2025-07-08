using Infrastructure.Services.AiServices.Salaries;

namespace Infrastructure.Services.AiServices.Custom;

public interface ICustomOpenAiService
{
    string GetBearer();

    Task<string> GetAnalysisAsync(
        SalariesAiBodyReport report,
        CancellationToken cancellationToken = default);
}