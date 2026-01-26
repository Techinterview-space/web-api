using System;
using System.Collections.Generic;

namespace Web.Api.Features.Currencies.Dtos;

public record WeeklyCurrencyChartDataDto
{
    public DateTime WeekStartDate { get; }

    public DateTime WeekEndDate { get; }

    public List<CurrencyItemDto> AverageCurrencies { get; }

    public WeeklyCurrencyChartDataDto(
        DateTime weekStartDate,
        DateTime weekEndDate,
        List<CurrencyItemDto> averageCurrencies)
    {
        WeekStartDate = weekStartDate;
        WeekEndDate = weekEndDate;
        AverageCurrencies = averageCurrencies;
    }
}
