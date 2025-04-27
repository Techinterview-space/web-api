using Domain.Entities.Companies;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Database.Config;

public class CompanyReviewEntityConfiguration : IEntityTypeConfiguration<CompanyReview>
{
    public void Configure(
        EntityTypeBuilder<CompanyReview> builder)
    {
        builder.ToTable("CompanyReviews");
        builder.HasKey(x => x.Id);

        builder
            .HasOne(x => x.Company)
            .WithMany(x => x.Reviews)
            .HasForeignKey(x => x.CompanyId);

        builder
            .HasOne(x => x.User)
            .WithMany(x => x.Reviews)
            .HasForeignKey(x => x.UserId);
    }
}