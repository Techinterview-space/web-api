using Domain.Entities.StatData;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Database.Config;

public class StatDataCacheEntityConfig : IEntityTypeConfiguration<StatDataCache>
{
    public void Configure(
        EntityTypeBuilder<StatDataCache> builder)
    {
        builder.ToTable(nameof(StatDataCache));
        builder.HasKey(x => x.Id);

        builder
            .Property(x => x.Name)
            .HasMaxLength(500)
            .IsRequired(true);

        builder
            .Property(x => x.ProfessionIds)
            .HasJsonConversion();
    }
}