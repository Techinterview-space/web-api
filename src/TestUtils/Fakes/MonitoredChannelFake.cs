using System.Threading.Tasks;
using Domain.Entities.ChannelStats;
using Microsoft.EntityFrameworkCore;
using TestUtils.Db;

namespace TestUtils.Fakes;

public class MonitoredChannelFake : MonitoredChannel, IPlease<MonitoredChannel>
{
    public MonitoredChannelFake(
        long channelExternalId = -1001234567890,
        string channelName = "Test Channel",
        long? discussionChatExternalId = -1009876543210)
        : base(channelExternalId, channelName, discussionChatExternalId)
    {
    }

    public MonitoredChannelFake AsInactive()
    {
        Deactivate();
        return this;
    }

    public MonitoredChannel Please() => this;

    public IPlease<MonitoredChannel> AsPlease() => this;

    public async Task<MonitoredChannel> PleaseAsync(DbContext context)
    {
        var entry = await context.AddAsync(Please());
        await context.SaveChangesAsync();
        return entry.Entity;
    }
}
