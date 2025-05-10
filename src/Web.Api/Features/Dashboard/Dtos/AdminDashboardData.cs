namespace Web.Api.Features.Dashboard.Dtos;

public record AdminDashboardData
{
    public AdminDashboardData(
        AverageRatingData averageRatingData,
        int totalSalaries,
        int totalCompanyReviews)
    {
        AverageRatingData = averageRatingData;
        TotalSalaries = totalSalaries;
        TotalCompanyReviews = totalCompanyReviews;
    }

    public AverageRatingData AverageRatingData { get; }

    public int TotalSalaries { get; }

    public int TotalCompanyReviews { get; }
}