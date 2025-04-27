using Domain.Entities.Companies;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Database.Config;

public class CompanyRatingHistoryEntityConfiguration : IEntityTypeConfiguration<CompanyRatingHistory>
{
    public void Configure(
        EntityTypeBuilder<CompanyRatingHistory> builder)
    {
        builder.ToTable("CompanyRatingHistory");
        builder.HasKey(x => x.Id);

        builder
            .HasOne(x => x.Company)
            .WithMany(x => x.RatingHistory)
            .HasForeignKey(x => x.CompanyId);
    }
}