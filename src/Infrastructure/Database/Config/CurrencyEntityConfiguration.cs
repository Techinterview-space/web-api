using Domain.Entities.Currencies;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Database.Config;

public class CurrencyEntityConfiguration : IEntityTypeConfiguration<CurrencyEntity>
{
    public void Configure(
        EntityTypeBuilder<CurrencyEntity> builder)
    {
        builder.ToTable("CurrencyEntities");
        builder.HasKey(x => x.Id);

        builder
            .HasIndex(x => new
            {
                x.ForDate,
                x.Currency,
            })
            .IsUnique();
    }
}