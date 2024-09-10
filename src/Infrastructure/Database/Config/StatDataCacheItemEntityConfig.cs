using Domain.Entities.StatData;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Database.Config;

public class StatDataCacheItemEntityConfig : IEntityTypeConfiguration<StatDataCacheItem>
{
    public void Configure(EntityTypeBuilder<StatDataCacheItem> builder)
    {
        builder.ToTable($"{nameof(StatDataCacheItem)}s");
        builder.HasKey(x => x.Id);

        builder
            .HasOne(x => x.StatDataCache)
            .WithMany(x => x.StatDataCacheItems)
            .HasForeignKey(x => x.StatDataCacheId)
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .HasOne(x => x.PreviousStatDataCacheItem)
            .WithMany(x => x.NextStatDataCacheItems)
            .HasForeignKey(x => x.PreviousStatDataCacheItemId)
            .OnDelete(DeleteBehavior.SetNull);

        builder
            .Property(x => x.Data)
            .HasJsonConversion();
    }
}