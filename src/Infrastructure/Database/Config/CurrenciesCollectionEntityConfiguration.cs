using Domain.Entities.Currencies;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Database.Config;

public class CurrenciesCollectionEntityConfiguration : IEntityTypeConfiguration<CurrenciesCollection>
{
    public void Configure(
        EntityTypeBuilder<CurrenciesCollection> builder)
    {
        builder.ToTable("CurrenciesCollections");
        builder.HasKey(x => x.Id);

        builder
            .Property(x => x.Currencies)
            .HasJsonConversion();

        builder
            .HasIndex(x => x.CurrencyDate)
            .IsUnique();
    }
}