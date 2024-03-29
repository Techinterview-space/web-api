﻿using System.Text;
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

    public async Task<FileData> RenderAsync(Interview interview, CancellationToken cancellationToken = default)
        => await _pdf.RenderAsync(PrepareHtml(interview), Filename(interview), FileData.PDfContentType, cancellationToken);

    public FileData Render(Interview interview)
        => _pdf.Render(PrepareHtml(interview), Filename(interview), FileData.PDfContentType);

    private string PrepareHtml(
        Interview interview)
    {
        var builder = new StringBuilder();
        builder.Append(new InterviewMarkdown(interview));
        builder.Append(GetTemplateFooter(interview));
        return _generator.FromMarkdown(builder.ToString());
    }

    private string Filename(
        Interview interview) =>
        $"Feedback_for_{interview.CandidateName}.pdf";

    private string GetTemplateFooter(
        Interview interview)
    {
        var builder = new StringBuilder();
        builder.AppendLine();
        builder.AppendLine(MarkdownItems.Line());
        builder.AppendLine();
        builder.AppendLine(MarkdownItems.Italic($"View this interview {MarkdownItems.Link(_global.InterviewWebLink(interview.Id), "on website")}"));
        builder.AppendLine();
        builder.AppendLine(MarkdownItems.Italic(MarkdownItems.Link(_global.FrontendBaseUrl, _global.AppName)));
        builder.AppendLine();
        builder.AppendLine(MarkdownItems.Italic(_global.AppVersion));
        builder.AppendLine();
        builder.AppendLine(MarkdownItems.Italic(DateTimeOffset.Now.ToString("yyyy-MM-dd HH:mm:ss zz")));
        builder.AppendLine();

        return builder.ToString();
    }
}