using Infrastructure.Services.OpenAi.Custom.Models;

namespace Infrastructure.Services.OpenAi.Custom;

public interface ICustomOpenAiService
{
    string GetBearer();

    Task<string> GetAnalysisAsync(
        OpenAiBodyReport report,
        CancellationToken cancellationToken = default);
}