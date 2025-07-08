using Infrastructure.Services.AiServices.Salaries;

namespace Web.Api.Features.SalarySubscribtions.GetOpenAiReportAnalysis;

public record GetOpenAiReportAnalysisResponse
{
    public GetOpenAiReportAnalysisResponse(
        string analysisRaw,
        string analysisHtml,
        SalariesAiBodyReport report,
        string model)
    {
        Analysis = analysisRaw;
        Html = analysisHtml;
        Report = report;
        Model = model;
    }

    public string Analysis { get; }

    public string Html { get; }

    public SalariesAiBodyReport Report { get; }

    public string Model { get; }
}