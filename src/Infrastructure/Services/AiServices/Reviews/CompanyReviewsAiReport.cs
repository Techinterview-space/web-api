namespace Infrastructure.Services.AiServices.Reviews;

public record CompanyReviewsAiReport
{
    public string ToJson()
    {
        return System.Text.Json.JsonSerializer.Serialize(this);
    }
}