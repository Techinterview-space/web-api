using System;
using System.Collections.Generic;
using Domain.Entities.Salaries;
using Domain.ValueObjects;
using Domain.ValueObjects.Dates;

namespace Web.Api.Features.Historical.GetSurveyHistoricalChart;

public record GetSurveyHistoricalChartResponse
{
    private GetSurveyHistoricalChartResponse()
    {
    }

    public GetSurveyHistoricalChartResponse(
        List<SurveyDatabaseData> records,
        DateTimeOffset from,
        DateTimeOffset to,
        DateTimeOffset? lastSurveyReplyDate)
    {
        From = from;
        To = to;
        ChartFrom = from;
        ChartTo = to;
        LastSurveyReplyDate = lastSurveyReplyDate;

        var someWeeksAgo = to.DateTime.AddYears(-1);
        var firstSalaryDate = records[0].CreatedAt;
        var fromDateToCalculateWeeks = firstSalaryDate.Earlier(someWeeksAgo)
            ? someWeeksAgo
            : firstSalaryDate.DateTime;

        var weekSplitterFrom = from.Earlier(fromDateToCalculateWeeks)
            ? someWeeksAgo
            : from.DateTime;

        var rangeSplitter = new DateTimeRangeSplitter(
            weekSplitterFrom,
            to.DateTime,
            TimeSpan.FromDays(15));

        var localSurveyReplies = new List<SurveyDatabaseData>();
        var remoteSurveyReplies = new List<SurveyDatabaseData>();

        foreach (var salary in records)
        {
            if (salary.LastSalaryOrNull?.CompanyType is CompanyType.Local)
            {
                localSurveyReplies.Add(salary);
            }
            else if (salary.LastSalaryOrNull?.CompanyType is CompanyType.Foreign)
            {
                remoteSurveyReplies.Add(salary);
            }
        }

        SurveyResultsByWeeksChart = new SurveyResultsByWeeksChart(
            localSurveyReplies,
            remoteSurveyReplies,
            rangeSplitter);
    }

    public bool HasRecentSurveyReply => LastSurveyReplyDate.HasValue;

    public DateTimeOffset? LastSurveyReplyDate { get; }

    public SurveyResultsByWeeksChart SurveyResultsByWeeksChart { get; private set; }

    public bool ShouldAddOwnSalary { get; private set; }

    public bool HasAuthentication { get; private set; }

    public DateTimeOffset From { get; private set; }

    public DateTimeOffset To { get; private set; }

    public DateTimeOffset ChartFrom { get; private set; }

    public DateTimeOffset ChartTo { get; private set; }

    public static GetSurveyHistoricalChartResponse Empty(
        DateTimeOffset from,
        DateTimeOffset to)
    {
        return new GetSurveyHistoricalChartResponse
        {
            HasAuthentication = true,
            ShouldAddOwnSalary = false,
            From = from,
            To = to,
            ChartFrom = from,
            ChartTo = to,
        };
    }

    public static GetSurveyHistoricalChartResponse NoSalaryOrAuthorization(
        bool hasAuthentication,
        bool shouldAddOwnSalary,
        DateTimeOffset from,
        DateTimeOffset to)
    {
        return new GetSurveyHistoricalChartResponse
        {
            HasAuthentication = hasAuthentication,
            ShouldAddOwnSalary = shouldAddOwnSalary,
            From = from,
            To = to,
            ChartFrom = from,
            ChartTo = to,
        };
    }
}