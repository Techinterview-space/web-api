using System.Collections.Generic;
using System.Linq;
using Domain.Extensions;

namespace Domain.Entities.StatData;

// mgorbatyuk: old props should not be renamed.
public record StatDataCacheItemSalaryData
{
    public StatDataCacheItemSalaryData()
    {
    }

    public StatDataCacheItemSalaryData(
        List<double> values,
        int totalCount)
    {
        MedianLocalSalary = values.Median();
        AverageLocalSalary = values.Count > 0 ? values.Average() : 0;
        TotalSalaryCount = totalCount;
    }

    public double MedianLocalSalary { get; init; }

    public double AverageLocalSalary { get; init; }

    public int TotalSalaryCount { get; init; }
}