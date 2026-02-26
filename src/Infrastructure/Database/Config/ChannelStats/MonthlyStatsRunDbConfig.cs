using Domain.Entities.ChannelStats;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Database.Config.ChannelStats;

internal class MonthlyStatsRunDbConfig : IEntityTypeConfiguration<MonthlyStatsRun>
{
    public void Configure(EntityTypeBuilder<MonthlyStatsRun> builder)
    {
        builder.HasIndex(x => new { x.MonitoredChannelId, x.Month });

        builder
            .HasOne(x => x.MonitoredChannel)
            .WithMany()
            .HasForeignKey(x => x.MonitoredChannelId)
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .Property(x => x.MostLikedPostRef)
            .HasMaxLength(500);

        builder
            .Property(x => x.MostCommentedPostRef)
            .HasMaxLength(500);
    }
}
