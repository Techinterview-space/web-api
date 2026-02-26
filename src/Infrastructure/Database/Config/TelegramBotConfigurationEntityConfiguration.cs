using Domain.Entities.Telegram;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Database.Config;

public class TelegramBotConfigurationEntityConfiguration
    : IEntityTypeConfiguration<TelegramBotConfiguration>
{
    public void Configure(
        EntityTypeBuilder<TelegramBotConfiguration> builder)
    {
        builder.ToTable("TelegramBotConfigurations");
        builder.HasKey(x => x.Id);

        builder
            .Property(x => x.Id)
            .ValueGeneratedNever();

        builder
            .Property(x => x.BotType)
            .IsRequired();

        builder
            .HasIndex(x => x.BotType)
            .IsUnique();

        builder
            .Property(x => x.DisplayName)
            .IsRequired()
            .HasMaxLength(200);

        builder
            .Property(x => x.Token)
            .IsRequired()
            .HasMaxLength(500);

        builder
            .Property(x => x.BotUsername)
            .IsRequired(false)
            .HasMaxLength(200);
    }
}
