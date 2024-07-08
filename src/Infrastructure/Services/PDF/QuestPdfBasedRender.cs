using Infrastructure.Services.Files;
using QuestPDF.Fluent;

namespace Infrastructure.Services.PDF;

public class QuestPdfBasedRender : IPdf
{
    private bool _disposed;

    public Task<FileData> RenderAsync(
        string htmlContent,
        string filename,
        string contentType,
        CancellationToken cancellationToken = default)
    {
        return Task.Run(() => Render(htmlContent, filename, contentType), cancellationToken);
    }

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
                page.Content().Text(htmlContent);
            });
        });

        var pdf = document.GeneratePdf();
        return new FileData(
            pdf,
            filename,
            contentType);
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_disposed || !disposing)
        {
            return;
        }

        _disposed = true;
    }
}