using Domain.Entities.Interviews;
using Infrastructure.Services.Files;
using Infrastructure.Services.Global;
using Infrastructure.Services.Html;

namespace Infrastructure.Services.PDF.Interviews;

public class InterviewPdfService : IInterviewPdfService
{
    private readonly IGlobal _global;
    private readonly IPdf _pdf;
    private readonly ITechInterviewHtmlGenerator _generator;

    public InterviewPdfService(
        IGlobal global,
        IPdf pdf,
        ITechInterviewHtmlGenerator generator)
    {
        _global = global;
        _pdf = pdf;
        _generator = generator;
    }

    public async Task<FileData> RenderAsync(
        Interview interview,
        CancellationToken cancellationToken = default)
        => await _pdf.RenderAsync(
            PrepareHtml(interview),
            Filename(interview),
            FileData.PDfContentType,
            cancellationToken);

    public FileData Render(
        Interview interview)
        => _pdf.Render(
            PrepareHtml(interview),
            Filename(interview),
            FileData.PDfContentType);

    private string PrepareHtml(
        Interview interview)
    {
        var content = new InterviewMarkdownBody(interview)
            .WithFooter(_global)
            .ToString();

        return _generator.FromMarkdown(content);
    }

    private string Filename(
        Interview interview) =>
        $"Feedback_for_{interview.CandidateName}.pdf";
}