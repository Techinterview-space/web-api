using System.Threading;
using System.Threading.Tasks;
using Domain.Entities.Interviews;
using Domain.Files;

namespace Domain.Services.Interviews;

public interface IInterviewPdf
{
    Task<FileData> RenderAsync(Interview interview, CancellationToken cancellationToken = default);

    FileData Render(Interview interview);
}