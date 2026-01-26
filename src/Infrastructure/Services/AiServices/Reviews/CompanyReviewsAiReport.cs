namespace Infrastructure.Services.AiServices.Reviews;

public record CompanyReviewsAiReport
{
    public List<CompanyReviewAiReportItem> Reviews { get; }

    public CompanyReviewsAiReport(
        List<CompanyReviewAiReportItem> reviews)
    {
        Reviews = reviews ?? throw new ArgumentNullException(nameof(reviews));
    }

    public string ToJson()
    {
        return System.Text.Json.JsonSerializer.Serialize(this);
    }
}