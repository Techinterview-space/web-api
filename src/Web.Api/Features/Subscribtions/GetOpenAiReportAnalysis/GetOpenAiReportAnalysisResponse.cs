using Infrastructure.Services.AiServices.Custom.Models;

namespace Web.Api.Features.Subscribtions.GetOpenAiReportAnalysis;

public record GetOpenAiReportAnalysisResponse
{
    public GetOpenAiReportAnalysisResponse(
        string analysis,
        OpenAiBodyReport report,
        string model)
    {
        Analysis = analysis;
        Report = report;
        Model = model;
    }

    public string Analysis { get; }

    public OpenAiBodyReport Report { get; }

    public string Model { get; }
}