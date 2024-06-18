using System;
using System.Collections.Generic;
using System.Linq;
using Domain.Entities.Enums;
using Domain.Extensions;
using Domain.ValueObjects;
using Infrastructure.Salaries;

namespace Web.Api.Features.Historical.GetSalariesHistoricalChart.Charts;

public record SalariesCountWeekByWeekChart
{
    public List<DateTime> WeekEnds { get; }

    public List<SalariesCountWeekByWeekChartItem> TotalCountItems { get; }

    public List<SalariesCountWeekByWeekChartGradeItem> GradeItems { get; }

    public bool HasGradeItems => GradeItems.Count > 0;

    public SalariesCountWeekByWeekChart(
        List<UserSalarySimpleDto> localSalaries,
        List<UserSalarySimpleDto> remoteSalaries,
        DateTimeRangeSplitter weekSplitter,
        bool addGradeChartData)
    {
        WeekEnds = new List<DateTime>();
        TotalCountItems = new List<SalariesCountWeekByWeekChartItem>();
        GradeItems = new List<SalariesCountWeekByWeekChartGradeItem>();

        foreach (var (start, end) in weekSplitter.ToList())
        {
            WeekEnds.Add(end);

            var localSalariesForPeriod = localSalaries
                .Where(x => x.CreatedAt <= end)
                .ToList();

            var remoteSalariesForPeriod = remoteSalaries
                .Where(x => x.CreatedAt <= end)
                .ToList();

            TotalCountItems.Add(new SalariesCountWeekByWeekChartItem(
                localSalariesForPeriod.Count + remoteSalariesForPeriod.Count,
                localSalariesForPeriod.Median(x => x.Value),
                localSalariesForPeriod.AverageOrDefault(x => x.Value),
                remoteSalariesForPeriod.Median(x => x.Value),
                remoteSalariesForPeriod.AverageOrDefault(x => x.Value)));

            if (!addGradeChartData)
            {
                continue;
            }

            foreach (var grade in GetSalariesHistoricalChartResponse.GradesToBeUsedInChart)
            {
                var localByGrade = localSalariesForPeriod.Where(x => x.Grade == grade).ToList();
                var remoteByGrade = remoteSalariesForPeriod.Where(x => x.Grade == grade).ToList();

                GradeItems.Add(
                    new SalariesCountWeekByWeekChartGradeItem(
                        grade,
                        localByGrade.Count + remoteByGrade.Count,
                        localByGrade.Median(x => x.Value),
                        localByGrade.AverageOrDefault(x => x.Value),
                        remoteByGrade.Median(x => x.Value),
                        remoteByGrade.AverageOrDefault(x => x.Value)));
            }
        }
    }

#pragma warning disable SA1009
#pragma warning disable SA1313
    public record SalariesCountWeekByWeekChartItem(
        int TotalCount,
        double LocalMedian,
        double LocalAverage,
        double RemoteMedian,
        double RemoteAverage);

    public record SalariesCountWeekByWeekChartGradeItem(
        DeveloperGrade Grade,
        int TotalCount,
        double LocalMedian,
        double LocalAverage,
        double RemoteMedian,
        double RemoteAverage) : SalariesCountWeekByWeekChartItem(
            TotalCount,
            LocalMedian,
            LocalAverage,
            RemoteMedian,
            RemoteAverage);
}