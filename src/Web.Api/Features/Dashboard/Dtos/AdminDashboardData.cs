namespace Web.Api.Features.Dashboard.Dtos;

public record AdminDashboardData
{
    public AdminDashboardData(
        AverageRatingData averageRatingData,
        TelegramInlineUsagesData telegramInlineUsagesData)
    {
        AverageRatingData = averageRatingData;
        TelegramInlineUsagesData = telegramInlineUsagesData;
    }

    public AverageRatingData AverageRatingData { get; }

    public TelegramInlineUsagesData TelegramInlineUsagesData { get; }
}