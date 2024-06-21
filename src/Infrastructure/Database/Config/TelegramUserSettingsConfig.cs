using Domain.Entities.Telegram;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Database.Config;

public class TelegramUserSettingsConfig : IEntityTypeConfiguration<TelegramUserSettings>
{
    public void Configure(
        EntityTypeBuilder<TelegramUserSettings> builder)
    {
        builder.ToTable("TelegramUserSettings");
        builder.HasKey(x => x.Id);

        builder
            .Property(x => x.Username)
            .IsRequired(true);

        builder
            .Property(x => x.ChatId)
            .IsRequired(true);

        builder
            .HasIndex(x => x.ChatId)
            .IsUnique(true);
    }
}