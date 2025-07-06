using Infrastructure.Services.AiServices.Custom.Models;
using Infrastructure.Services.Html;

namespace Web.Api.Features.Subscribtions.GetOpenAiReportAnalysis;

public record GetOpenAiReportAnalysisResponse
{
    public GetOpenAiReportAnalysisResponse(
        string analysis,
        OpenAiBodyReport report,
        string model)
    {
        Analysis = analysis;
        Html = Analysis != null
            ? new MarkdownToHtml(Analysis).ToString()
            : null;

        Report = report;
        Model = model;
    }

    public string Analysis { get; }

    public string Html { get; }

    public OpenAiBodyReport Report { get; }

    public string Model { get; }
}