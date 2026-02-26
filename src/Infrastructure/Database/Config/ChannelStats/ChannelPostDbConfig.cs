using Domain.Entities.ChannelStats;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Database.Config.ChannelStats;

internal class ChannelPostDbConfig : IEntityTypeConfiguration<ChannelPost>
{
    public void Configure(EntityTypeBuilder<ChannelPost> builder)
    {
        builder
            .HasIndex(x => new { x.MonitoredChannelId, x.TelegramMessageId })
            .IsUnique();

        builder.HasIndex(x => x.PostedAtUtc);

        builder
            .HasOne(x => x.MonitoredChannel)
            .WithMany()
            .HasForeignKey(x => x.MonitoredChannelId)
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .Property(x => x.PostReference)
            .HasMaxLength(500);

        builder
            .Property(x => x.TextPreview)
            .HasMaxLength(500);
    }
}
