using System;
using System.Collections.Generic;
using Web.Api.Features.Currencies.Dtos;

namespace Web.Api.Features.Currencies.GetCurrenciesChartData;

public record GetCurrenciesChartDataResponse
{
    public List<WeeklyCurrencyChartDataDto> WeeklyData { get; }

    public DateTime FromDate { get; }

    public DateTime ToDate { get; }

    public GetCurrenciesChartDataResponse(
        List<WeeklyCurrencyChartDataDto> weeklyData,
        DateTime fromDate,
        DateTime toDate)
    {
        WeeklyData = weeklyData;
        FromDate = fromDate;
        ToDate = toDate;
    }
}
