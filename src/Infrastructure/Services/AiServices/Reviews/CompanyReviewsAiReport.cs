namespace Infrastructure.Services.AiServices.Reviews;

public record CompanyReviewsAiReport
{
    public CompanyReviewsAiReport()
    {
        throw new NotImplementedException();
    }

    public string ToJson()
    {
        return System.Text.Json.JsonSerializer.Serialize(this);
    }
}