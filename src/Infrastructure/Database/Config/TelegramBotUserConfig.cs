using Domain.Entities.Telegram;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Database.Config;

internal class TelegramBotUserConfig : IEntityTypeConfiguration<TelegramBotUsage>
{
    public void Configure(
        EntityTypeBuilder<TelegramBotUsage> builder)
    {
        builder.ToTable("TelegramBotUsages");
        builder.HasKey(x => x.Id);

        builder
            .Property(x => x.UsageCount)
            .HasDefaultValue(0);

        builder
            .Property(x => x.Username)
            .IsRequired(true);

        builder
            .HasIndex(x => new
            {
                x.Username,
                x.UsageType,
            })
            .IsUnique(false);
    }
}