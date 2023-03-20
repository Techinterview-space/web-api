using System;
using System.Threading;
using System.Threading.Tasks;

namespace Domain.Files;

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