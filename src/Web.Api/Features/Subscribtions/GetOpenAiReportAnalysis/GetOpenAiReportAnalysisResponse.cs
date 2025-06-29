using Infrastructure.Services.OpenAi.Custom.Models;

namespace Web.Api.Features.Subscribtions.GetOpenAiReportAnalysis;

public record GetOpenAiReportAnalysisResponse
{
    public GetOpenAiReportAnalysisResponse(
        string analysis,
        OpenAiBodyReport report,
        string bearer)
    {
        Analysis = analysis;
        Report = report;
        Bearer = bearer;
    }

    public string Analysis { get; }

    public OpenAiBodyReport Report { get; }

    public string Bearer { get; }
}