using Infrastructure.Services.AiServices.Custom.Models;

namespace Web.Api.Features.SalarySubscribtions.GetOpenAiReportAnalysis;

public record GetOpenAiReportAnalysisResponse
{
    public GetOpenAiReportAnalysisResponse(
        string analysisRaw,
        string analysisHtml,
        OpenAiBodyReport report,
        string model)
    {
        Analysis = analysisRaw;
        Html = analysisHtml;
        Report = report;
        Model = model;
    }

    public string Analysis { get; }

    public string Html { get; }

    public OpenAiBodyReport Report { get; }

    public string Model { get; }
}