namespace Infrastructure.Services.Files;

public interface IPdf : IDisposable
{
    Task<FileData> RenderAsync(
        string htmlContent,
        string filename,
        string contentType,
        CancellationToken cancellationToken = default);

    FileData Render(
        string htmlContent,
        string filename,
        string contentType);
}