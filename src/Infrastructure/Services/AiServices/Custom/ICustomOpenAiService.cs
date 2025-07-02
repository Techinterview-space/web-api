using Infrastructure.Services.AiServices.Custom.Models;

namespace Infrastructure.Services.AiServices.Custom;

public interface ICustomOpenAiService
{
    string GetBearer();

    Task<string> GetAnalysisAsync(
        OpenAiBodyReport report,
        CancellationToken cancellationToken = default);
}