using System;
using System.Collections.Generic;

namespace Web.Api.Features.Historical.GetSurveyHistoricalChart;

public record GetSurveyHistoricalChartResponse
{
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
    }

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