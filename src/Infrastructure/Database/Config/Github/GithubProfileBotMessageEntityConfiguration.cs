using Domain.Entities.Github;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Database.Config.Github;

public class GithubProfileBotMessageEntityConfiguration : IEntityTypeConfiguration<GithubProfileBotMessage>
{
    public void Configure(
        EntityTypeBuilder<GithubProfileBotMessage> builder)
    {
        builder.ToTable("GithubProfileBotMessages");
        builder.HasKey(x => x.Id);

        builder
            .HasOne(x => x.GithubProfileBotChat)
            .WithMany(x => x.Messages)
            .HasForeignKey(x => x.GithubProfileBotChatId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}