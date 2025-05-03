using Infrastructure.Services.OpenAi.Models;

namespace Infrastructure.Services.OpenAi;

public interface IOpenAiService
{
    string GetBearer();

    Task<string> GetAnalysisAsync(
        OpenAiBodyReport report,
        CancellationToken cancellationToken = default);
}