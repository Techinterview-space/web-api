namespace Infrastructure.Services.OpenAi;

public interface IOpenAiService
{
    string GetBearer();

    Task<string> GetAnalysisAsync(
        CancellationToken cancellationToken = default);
}