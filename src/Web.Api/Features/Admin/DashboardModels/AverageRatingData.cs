using System.Collections.Generic;
using System.Linq;

namespace Web.Api.Features.Admin.DashboardModels;

public record AverageRatingData
{
    public int Count { get; }

    public double AverageRating { get; }

    public AverageRatingData(
        List<int> averageRatingItems)
    {
        if (averageRatingItems.Count == 0)
        {
            Count = 0;
            AverageRating = 0;
        }
        else
        {
            Count = averageRatingItems.Count;
            AverageRating = averageRatingItems.Average();
        }
    }
}