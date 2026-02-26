using Domain.Entities.ChannelStats;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Database.Config.ChannelStats;

internal class MonitoredChannelDbConfig : IEntityTypeConfiguration<MonitoredChannel>
{
    public void Configure(EntityTypeBuilder<MonitoredChannel> builder)
    {
        builder.HasIndex(x => x.ChannelExternalId).IsUnique();

        builder
            .HasIndex(x => x.DiscussionChatExternalId)
            .IsUnique();

        builder
            .Property(x => x.ChannelName)
            .HasMaxLength(200)
            .IsRequired();

        builder.HasIndex(x => x.IsActive);
    }
}
