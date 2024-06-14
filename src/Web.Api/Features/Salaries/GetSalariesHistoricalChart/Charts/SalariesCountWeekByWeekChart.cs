﻿using System;
using System.Collections.Generic;
using System.Linq;
using Domain.Entities.Enums;
using Domain.Extensions;
using Infrastructure.Salaries;

namespace TechInterviewer.Features.Salaries.GetSalariesHistoricalChart.Charts;

public record SalariesCountWeekByWeekChart
{
    public List<SalariesCountWeekByWeekChartItem> TotalCountItems { get; init; }

    public List<SalariesCountWeekByWeekChartGradeItem> GradeItems { get; init; }

    public SalariesCountWeekByWeekChart(
        List<UserSalarySimpleDto> localSalaries,
        List<UserSalarySimpleDto> remoteSalaries,
        WeekSplitter weekSplitter)
    {
        TotalCountItems = new List<SalariesCountWeekByWeekChartItem>();
        GradeItems = new List<SalariesCountWeekByWeekChartGradeItem>();

        foreach (var (start, end) in weekSplitter.ToList())
        {
            var localSalariesForPeriod = localSalaries
                .Where(x => x.CreatedAt <= end)
                .ToList();

            var remoteSalariesForPeriod = remoteSalaries
                .Where(x => x.CreatedAt <= end)
                .ToList();

            TotalCountItems.Add(new SalariesCountWeekByWeekChartItem(
                end,
                localSalariesForPeriod.Count + remoteSalariesForPeriod.Count,
                localSalariesForPeriod.Median(x => x.Value),
                localSalariesForPeriod.AverageOrDefault(x => x.Value),
                remoteSalariesForPeriod.Median(x => x.Value),
                remoteSalariesForPeriod.AverageOrDefault(x => x.Value)));

            foreach (var grade in GetSalariesHistoricalChartResponse.GradesToBeUsedInChart)
            {
                var localByGrade = localSalariesForPeriod.Where(x => x.Grade == grade).ToList();
                var remoteByGrade = remoteSalariesForPeriod.Where(x => x.Grade == grade).ToList();

                GradeItems.Add(
                    new SalariesCountWeekByWeekChartGradeItem(
                        grade,
                        end,
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
        DateTime WeekEnd,
        int TotalCount,
        double LocalMedian,
        double LocalAverage,
        double RemoteMedian,
        double RemoteAverage);

    public record SalariesCountWeekByWeekChartGradeItem(
        DeveloperGrade Grade,
        DateTime WeekEnd,
        int TotalCount,
        double LocalMedian,
        double LocalAverage,
        double RemoteMedian,
        double RemoteAverage) : SalariesCountWeekByWeekChartItem(
            WeekEnd,
            TotalCount,
            LocalMedian,
            LocalAverage,
            RemoteMedian,
            RemoteAverage);
}