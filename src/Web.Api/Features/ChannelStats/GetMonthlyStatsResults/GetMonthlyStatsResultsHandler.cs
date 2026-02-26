using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Web.Api.Features.ChannelStats.RunMonthlyStats;

namespace Web.Api.Features.ChannelStats.GetMonthlyStatsResults;

public class GetMonthlyStatsResultsHandler
    : Infrastructure.Services.Mediator.IRequestHandler<GetMonthlyStatsResultsQuery, List<MonthlyStatsRunDto>>
{
    private readonly DatabaseContext _context;

    public GetMonthlyStatsResultsHandler(DatabaseContext context)
    {
        _context = context;
    }

    public async Task<List<MonthlyStatsRunDto>> Handle(
        GetMonthlyStatsResultsQuery request,
        CancellationToken cancellationToken)
    {
        var monthStartUtc = new DateTimeOffset(
            request.Year,
            request.Month,
            1,
            0,
            0,
            0,
            TimeSpan.Zero);

        var monthEndUtc = monthStartUtc.AddMonths(1);

        var runs = await _context.MonthlyStatsRuns
            .AsNoTracking()
            .Include(x => x.MonitoredChannel)
            .Where(x => x.Month >= monthStartUtc && x.Month < monthEndUtc)
            .OrderByDescending(x => x.CalculatedAtUtc)
            .ToListAsync(cancellationToken);

        return runs
            .Select(x => new MonthlyStatsRunDto(x))
            .ToList();
    }
}
