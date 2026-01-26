using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Domain.Entities.Salaries;
using Infrastructure.Database;
using Infrastructure.Services.Mediator;
using Microsoft.EntityFrameworkCore;
using Web.Api.Features.Currencies.Dtos;

namespace Web.Api.Features.Currencies.GetCurrenciesChartData;

public class GetCurrenciesChartDataHandler
    : IRequestHandler<GetCurrenciesChartDataQueryParams, GetCurrenciesChartDataResponse>
{
    private readonly DatabaseContext _context;

    public GetCurrenciesChartDataHandler(DatabaseContext context)
    {
        _context = context;
    }

    public async Task<GetCurrenciesChartDataResponse> Handle(
        GetCurrenciesChartDataQueryParams request,
        CancellationToken cancellationToken)
    {
        var toDate = DateTime.UtcNow.Date;
        var fromDate = toDate.AddYears(-1);

        var currencyRecords = await _context.CurrencyCollections
            .AsNoTracking()
            .Where(x => x.CurrencyDate >= fromDate && x.CurrencyDate <= toDate)
            .OrderBy(x => x.CurrencyDate)
            .ToListAsync(cancellationToken);

        var weeklyData = currencyRecords
            .GroupBy(x => GetStartOfWeek(x.CurrencyDate))
            .Select(weekGroup =>
            {
                var weekRecords = weekGroup.ToList();
                var weekStart = weekGroup.Key;
                var weekEnd = weekStart.AddDays(6);

                var allCurrencies = weekRecords
                    .SelectMany(r => r.Currencies)
                    .GroupBy(kvp => kvp.Key)
                    .Select(currencyGroup => new CurrencyItemDto
                    {
                        Currency = currencyGroup.Key,
                        Value = currencyGroup.Average(kvp => kvp.Value)
                    })
                    .ToList();

                return new WeeklyCurrencyChartDataDto(
                    weekStart,
                    weekEnd,
                    allCurrencies);
            })
            .OrderBy(x => x.WeekStartDate)
            .ToList();

        return new GetCurrenciesChartDataResponse(
            weeklyData,
            fromDate,
            toDate);
    }

    private static DateTime GetStartOfWeek(DateTime date)
    {
        var diff = (7 + (date.DayOfWeek - DayOfWeek.Monday)) % 7;
        return date.AddDays(-1 * diff).Date;
    }
}
