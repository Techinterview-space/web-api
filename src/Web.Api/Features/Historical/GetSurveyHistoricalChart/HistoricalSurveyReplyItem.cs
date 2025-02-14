namespace Web.Api.Features.Historical.GetSurveyHistoricalChart;

public record HistoricalSurveyReplyItem
{
    public HistoricalSurveyReplyItem(
        int ratingValue,
        double percentage)
    {
        RatingValue = ratingValue;
        Percentage = percentage;
    }

    public int RatingValue { get; }

    public double Percentage { get; }
}