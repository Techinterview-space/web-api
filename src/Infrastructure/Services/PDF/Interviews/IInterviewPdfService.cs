using Domain.Entities.Interviews;
using Infrastructure.Services.Files;

namespace Infrastructure.Services.PDF.Interviews;

public interface IInterviewPdfService
{
    FileData Render(
        Interview interview);
}