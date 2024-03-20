using Domain.Entities.Interviews;
using Infrastructure.Services.Files;

namespace Infrastructure.Services.PDF.Interviews;

public interface IInterviewPdfService
{
    Task<FileData> RenderAsync(
        Interview interview,
        CancellationToken cancellationToken = default);

    FileData Render(
        Interview interview);
}