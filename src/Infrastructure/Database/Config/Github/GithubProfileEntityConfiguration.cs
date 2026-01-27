using Domain.Entities.Github;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Database.Config.Github;

public class GithubProfileEntityConfiguration : IEntityTypeConfiguration<GithubProfile>
{
    public void Configure(
        EntityTypeBuilder<GithubProfile> builder)
    {
        builder.ToTable("GithubProfiles");
        builder.HasKey(x => x.Username);

        builder
            .Property(x => x.Username)
            .IsRequired(true);

        builder
            .Property(x => x.Data)
            .HasJsonConversion();
    }
}