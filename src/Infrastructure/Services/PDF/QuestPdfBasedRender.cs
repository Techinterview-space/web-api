using Infrastructure.Services.Files;
using QuestPDF.Fluent;

namespace Infrastructure.Services.PDF;

public class QuestPdfBasedRender : IPdf
{
    public FileData Render(
        string htmlContent,
        string filename,
        string contentType = FileData.PDfContentType)
    {
        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Margin(20);
                var content = page.Content();
                content.Text(htmlContent);
            });
        });

        var pdf = document.GeneratePdf();
        return new FileData(
            pdf,
            filename,
            contentType);
    }
}