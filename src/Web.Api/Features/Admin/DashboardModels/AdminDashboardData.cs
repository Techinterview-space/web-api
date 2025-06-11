using System;
using System.Collections.Generic;

namespace Web.Api.Features.Admin.DashboardModels;

public record AdminDashboardData
{
    public AdminDashboardData(
        AverageRatingData averageRatingData,
        int totalSalaries,
        int totalCompanyReviews,
        int usersWithUnsubscribeMeFromAllCount,
        List<DateTimeOffset> userEmailsSourceData,
        List<DateTimeOffset> reviewLikesForLastDays,
        List<DateTimeOffset> reviewsForLastDays)
    {
        AverageRatingData = averageRatingData;
        TotalSalaries = totalSalaries;
        TotalCompanyReviews = totalCompanyReviews;
        UsersWithUnsubscribeMeFromAllCount = usersWithUnsubscribeMeFromAllCount;

        UserEmailsForLastDays = new ItemsPerDayChartData(userEmailsSourceData);
        ReviewLikesForLastTenDays = new ItemsPerDayChartData(reviewLikesForLastDays);
        ReviewsForLastTenDays = new ItemsPerDayChartData(reviewsForLastDays);
    }

    public AverageRatingData AverageRatingData { get; }

    public int TotalSalaries { get; }

    public int TotalCompanyReviews { get; }

    public int UsersWithUnsubscribeMeFromAllCount { get; }

    public ItemsPerDayChartData UserEmailsForLastDays { get; }

    public ItemsPerDayChartData ReviewLikesForLastTenDays { get; }

    public ItemsPerDayChartData ReviewsForLastTenDays { get; }
}