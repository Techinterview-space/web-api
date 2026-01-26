using Infrastructure.Services.AiServices.Reviews;

namespace Web.Api.Features.CompanyReviewsSubscriptions.GetOpenAiReportAnalysis;

public record GetCompanyReviewsAiReportAnalysisResponse
{
    public GetCompanyReviewsAiReportAnalysisResponse(
        string analysisRaw,
        string analysisHtml,
        CompanyReviewsAiReport report,
        string model)
    {
        Analysis = analysisRaw;
        Html = analysisHtml;
        Report = report;
        Model = model;
    }

    public string Analysis { get; }

    public string Html { get; }

    public CompanyReviewsAiReport Report { get; }

    public string Model { get; }
}