using System;
using System.Collections.Generic;
using System.Linq;
using Domain.Entities.Enums;
using Domain.Entities.Questions;
using Domain.Extensions;
using Domain.ValueObjects;
using Web.Api.Features.Historical.GetSalariesHistoricalChart;

namespace Web.Api.Features.Historical.GetSurveyHistoricalChart;

public record SurveyResultsByWeeksChart
{
    public List<DateTime> WeekEnds { get; }

    public List<SurveyResultsByWeeksChartItem> Items { get; }

    public List<SurveyResultsByWeeksChartGradeItem> GradeItems { get; }

    public bool HasGradeItems => GradeItems.Count > 0;

    public SurveyResultsByWeeksChart(
        List<SurveyDatabaseData> localRecords,
        List<SurveyDatabaseData> remoteRecords,
        DateTimeRangeSplitter weekSplitter,
        bool addGradeChartData)
    {
        WeekEnds = new List<DateTime>();
        Items = new List<SurveyResultsByWeeksChartItem>();
        GradeItems = new List<SurveyResultsByWeeksChartGradeItem>();

        foreach (var (start, end) in weekSplitter.ToList())
        {
            WeekEnds.Add(end);

            var localData = new RecordsForPeriodCalculator(
                localRecords,
                end);

            var remoteData = new RecordsForPeriodCalculator(
                remoteRecords,
                end);

            Items.Add(
                new SurveyResultsByWeeksChartItem(
                    localData.TotalCount + remoteData.TotalCount,
                    localData.GetUsefulnessReport(),
                    remoteData.GetUsefulnessReport()));

            if (!addGradeChartData)
            {
                continue;
            }

            foreach (var grade in GetSurveyHistoricalChartResponse.GradesToBeUsedInChart)
            {
                var localGradeData = new RecordsForPeriodCalculator(
                    localData
                        .RecordsForPeriod
                        .Where(x => x.LastSalaryOrNull.Grade == grade)
                        .ToList(),
                    end);

                var remoteGradeData = new RecordsForPeriodCalculator(
                    remoteData
                        .RecordsForPeriod
                        .Where(x => x.LastSalaryOrNull.Grade == grade)
                        .ToList(),
                    end);

                GradeItems.Add(
                    new SurveyResultsByWeeksChartGradeItem(
                        grade,
                        localGradeData.TotalCount,
                        remoteGradeData.TotalCount,
                        localGradeData.TotalCount + remoteGradeData.TotalCount,
                        localGradeData.GetUsefulnessReport(),
                        remoteGradeData.GetUsefulnessReport()));
            }
        }
    }

#pragma warning disable SA1009
#pragma warning disable SA1313
    public record SurveyResultsByWeeksChartItem(
        int TotalCount,
        List<HistoricalSurveyReplyItem> LocalUsefulnessPercentage,
        List<HistoricalSurveyReplyItem> RemoteUsefulnessPercentage);

    public record SurveyResultsByWeeksChartGradeItem(
        DeveloperGrade Grade,
        int LocalCount,
        int RemoteCount,
        int TotalCount,
        List<HistoricalSurveyReplyItem> LocalUsefulnessPercentage,
        List<HistoricalSurveyReplyItem> RemoteUsefulnessPercentage)
        : SurveyResultsByWeeksChartItem(
            TotalCount,
            LocalUsefulnessPercentage,
            RemoteUsefulnessPercentage);
}