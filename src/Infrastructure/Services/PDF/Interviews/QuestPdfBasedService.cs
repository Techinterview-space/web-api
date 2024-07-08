using Domain.Entities.Interviews;
using Infrastructure.Services.Files;
using Infrastructure.Services.Global;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Markdown;

namespace Infrastructure.Services.PDF.Interviews;

public class QuestPdfBasedService : IInterviewPdfService
{
    private readonly IGlobal _global;

    public QuestPdfBasedService(
        IGlobal global)
    {
        _global = global;
    }

    public Task<FileData> RenderAsync(
        Interview interview,
        CancellationToken cancellationToken = default)
    {
        return Task.Run(() => Render(interview), cancellationToken);
    }

    public FileData Render(
        Interview interview)
    {
        var options = new MarkdownRendererOptions
        {
            BlockQuoteBorderColor = Colors.Grey.Medium,
            BlockQuoteBorderThickness = 5
        };

        var content = new InterviewMarkdownBody(interview)
            .WithFooter(_global)
            .ToString();

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Margin(20);
                page.Content().Markdown(content, options);
            });
        });

        var pdf = document.GeneratePdf();
        return new FileData(
            pdf,
            Filename(interview),
            FileData.PDfContentType);
    }

    private string Filename(
        Interview interview) =>
        $"Feedback_for_{interview.CandidateName}.pdf";
}