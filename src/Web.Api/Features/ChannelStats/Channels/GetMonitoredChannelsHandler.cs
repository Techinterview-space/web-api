using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace Web.Api.Features.ChannelStats.Channels;

public class GetMonitoredChannelsHandler
    : Infrastructure.Services.Mediator.IRequestHandler<GetMonitoredChannelsQuery, List<MonitoredChannelDto>>
{
    private readonly DatabaseContext _context;

    public GetMonitoredChannelsHandler(DatabaseContext context)
    {
        _context = context;
    }

    public async Task<List<MonitoredChannelDto>> Handle(
        GetMonitoredChannelsQuery request,
        CancellationToken cancellationToken)
    {
        var channels = await _context.MonitoredChannels
            .AsNoTracking()
            .OrderBy(x => x.ChannelName)
            .ToListAsync(cancellationToken);

        return channels
            .Select(x => new MonitoredChannelDto(x))
            .ToList();
    }
}
