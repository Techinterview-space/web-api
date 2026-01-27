using Domain.Entities.Companies;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Database.Config;

public class CompanyEntityConfiguration : IEntityTypeConfiguration<Company>
{
    public void Configure(
        EntityTypeBuilder<Company> builder)
    {
        builder.ToTable("Companies");
        builder.HasKey(x => x.Id);

        builder
            .Property(x => x.Links)
            .HasJsonConversion();

        builder
            .Property(x => x.Name)
            .IsRequired();

        builder
            .Property(x => x.NormalizedName)
            .IsRequired();

        builder
            .Property(x => x.Description)
            .IsRequired();

        builder
            .HasIndex(x => x.NormalizedName)
            .IsUnique(false);

        builder
            .HasIndex(x => x.Slug)
            .IsUnique(true);
    }
}