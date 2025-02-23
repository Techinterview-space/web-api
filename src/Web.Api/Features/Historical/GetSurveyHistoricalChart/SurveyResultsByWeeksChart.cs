using System;
using System.Collections.Generic;
using System.Linq;
using Domain.ValueObjects;

namespace Web.Api.Features.Historical.GetSurveyHistoricalChart;

public record SurveyResultsByWeeksChart
{
    public List<DateTime> WeekEnds { get; }

    public List<double> LocalChartItems { get; }

    public List<double> RemoteChartItems { get; }

    public SurveyResultsByWeeksChart(
        List<SurveyDatabaseData> localRecords,
        List<SurveyDatabaseData> remoteRecords,
        DateTimeRangeSplitter weekSplitter)
    {
        WeekEnds = new List<DateTime>();
        LocalChartItems = new List<double>();
        RemoteChartItems = new List<double>();

        foreach (var (start, end) in weekSplitter.ToList())
        {
            WeekEnds.Add(end);

            var localDataForPeriod = localRecords
                .Where(x => x.CreatedAt >= start && x.CreatedAt < end)
                .ToList();

            var remoteDataForPeriod = remoteRecords
                .Where(x => x.CreatedAt >= start && x.CreatedAt < end)
                .ToList();

            if (localDataForPeriod.Count > 0)
            {
                LocalChartItems.Add(localDataForPeriod.Average(x => x.UsefulnessRating));
            }
            else
            {
                LocalChartItems.Add(0);
            }

            if (remoteDataForPeriod.Count > 0)
            {
                RemoteChartItems.Add(remoteDataForPeriod.Average(x => x.UsefulnessRating));
            }
            else
            {
                RemoteChartItems.Add(0);
            }
        }
    }
}