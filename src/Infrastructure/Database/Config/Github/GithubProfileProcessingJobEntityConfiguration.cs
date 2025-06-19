using Domain.Entities.Github;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Database.Config.Github;

public class GithubProfileProcessingJobEntityConfiguration : IEntityTypeConfiguration<GithubProfileProcessingJob>
{
    public void Configure(
        EntityTypeBuilder<GithubProfileProcessingJob> builder)
    {
        builder.ToTable("GithubProfileProcessingJobs");
        builder.HasKey(x => x.Username);
    }
}