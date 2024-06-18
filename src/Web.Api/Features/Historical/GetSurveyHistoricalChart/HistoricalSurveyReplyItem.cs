using System;

namespace Web.Api.Features.Historical.GetSurveyHistoricalChart;

public record HistoricalSurveyReplyItem<T>
    where T : Enum
{
    public HistoricalSurveyReplyItem(
        T replyType,
        double percentage)
    {
        ReplyType = replyType;
        Percentage = percentage;
    }

    public T ReplyType { get; }

    public double Percentage { get; }
}