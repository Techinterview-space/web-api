using Domain.Entities.Companies;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Database.Config;

public class CompanyReviewVoteEntityConfig : IEntityTypeConfiguration<CompanyReviewVote>
{
    public void Configure(
        EntityTypeBuilder<CompanyReviewVote> builder)
    {
        builder
            .HasKey(e => new { e.ReviewId, e.UserId });

        builder
            .HasOne(e => e.User)
            .WithMany(x => x.Votes)
            .HasForeignKey(e => e.UserId);

        builder
            .HasOne(e => e.CompanyReview)
            .WithMany(x => x.Votes)
            .HasForeignKey(e => e.ReviewId);
    }
}