using Domain.Entities.Github;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Database.Config.Github;

public class GithubPersonalUserTokenEntityConfiguration : IEntityTypeConfiguration<GithubPersonalUserToken>
{
    public void Configure(EntityTypeBuilder<GithubPersonalUserToken> builder)
    {
        builder.ToTable("GithubPersonalUserTokens");

        builder
            .HasKey(x => x.Id);

        builder
            .Property(x => x.Token)
            .IsRequired();

        builder
            .Property(x => x.ExpiresAt)
            .IsRequired();
    }
}