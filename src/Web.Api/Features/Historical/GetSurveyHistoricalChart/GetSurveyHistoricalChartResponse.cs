using System;
using System.Collections.Generic;
using Domain.Entities.Enums;
using Domain.Entities.Salaries;
using Domain.ValueObjects.Dates;

namespace Web.Api.Features.Historical.GetSurveyHistoricalChart;

public record GetSurveyHistoricalChartResponse
{
    public static readonly List<DeveloperGrade> GradesToBeUsedInChart = new ()
    {
        DeveloperGrade.Junior,
        DeveloperGrade.Middle,
        DeveloperGrade.Senior,
        DeveloperGrade.Lead,
    };

    private GetSurveyHistoricalChartResponse()
    {
    }

    public GetSurveyHistoricalChartResponse(
        List<SurveyDatabaseData> records,
        DateTimeOffset from,
        DateTimeOffset to)
    {
        From = from;
        To = to;
        ChartFrom = from;
        ChartTo = to;

        var someWeeksAgo = to.DateTime.AddDays(-140);
        var weekSplitterFrom = from.Earlier(someWeeksAgo)
            ? someWeeksAgo
            : from.DateTime;

        var weekSplitter = new WeekSplitter(weekSplitterFrom, to.DateTime);

        var localSalaries = new List<SurveyDatabaseData>();
        var remoteSalaries = new List<SurveyDatabaseData>();

        foreach (var salary in records)
        {
            if (salary.LastSalaryOrNull?.CompanyType is CompanyType.Local)
            {
                localSalaries.Add(salary);
            }
            else if (salary.LastSalaryOrNull?.CompanyType is CompanyType.Foreign)
            {
                remoteSalaries.Add(salary);
            }
        }

        SurveyResultsByWeeksChart = new SurveyResultsByWeeksChart(
            localSalaries,
            remoteSalaries,
            weekSplitter,
            true);
    }

    public SurveyResultsByWeeksChart SurveyResultsByWeeksChart { get; private set; }

    public bool ShouldAddOwnSalary { get; private set; }

    public bool HasAuthentication { get; private set; }

    public DateTimeOffset From { get; private set; }

    public DateTimeOffset To { get; private set; }

    public DateTimeOffset ChartFrom { get; private set; }

    public DateTimeOffset ChartTo { get; private set; }

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