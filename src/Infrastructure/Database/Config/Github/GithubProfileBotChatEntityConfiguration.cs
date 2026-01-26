using Domain.Entities.Github;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Database.Config.Github;

public class GithubProfileBotChatEntityConfiguration : IEntityTypeConfiguration<GithubProfileBotChat>
{
    public void Configure(
        EntityTypeBuilder<GithubProfileBotChat> builder)
    {
        builder.ToTable("GithubProfileBotChats");
        builder.HasKey(x => x.Id);

        builder
            .Property(x => x.ChatId)
            .IsRequired(true);

        builder
            .Property(x => x.Username)
            .IsRequired(true);

        builder
            .Property(x => x.IsAdmin)
            .IsRequired(true);

        builder
            .HasIndex(x => x.ChatId)
            .IsUnique();
    }
}