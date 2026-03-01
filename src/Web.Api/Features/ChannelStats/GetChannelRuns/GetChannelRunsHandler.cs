using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Web.Api.Features.ChannelStats.RunMonthlyStats;

namespace Web.Api.Features.ChannelStats.GetChannelRuns;

public class GetChannelRunsHandler
    : Infrastructure.Services.Mediator.IRequestHandler<GetChannelRunsQuery, List<MonthlyStatsRunDto>>
{
    private readonly DatabaseContext _context;

    public GetChannelRunsHandler(DatabaseContext context)
    {
        _context = context;
    }

    public async Task<List<MonthlyStatsRunDto>> Handle(
        GetChannelRunsQuery request,
        CancellationToken cancellationToken)
    {
        var take = Math.Clamp(request.Take, 1, 10);

        var runs = await _context.MonthlyStatsRuns
            .AsNoTracking()
            .Where(x => x.MonitoredChannelId == request.ChannelId)
            .OrderByDescending(x => x.CalculatedAtUtc)
            .Take(take)
            .ToListAsync(cancellationToken);

        return runs.Select(x => new MonthlyStatsRunDto(x)).ToList();
    }
}
