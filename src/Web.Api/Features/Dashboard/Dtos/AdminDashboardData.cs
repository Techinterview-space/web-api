namespace Web.Api.Features.Dashboard.Dtos;

public record AdminDashboardData
{
    public AdminDashboardData(
        AverageRatingData averageRatingData)
    {
        AverageRatingData = averageRatingData;
    }

    public AverageRatingData AverageRatingData { get; }
}