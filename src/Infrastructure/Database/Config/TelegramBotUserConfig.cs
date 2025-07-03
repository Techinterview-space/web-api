using Domain.Entities.Telegram;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Database.Config;

internal class TelegramBotUserConfig : IEntityTypeConfiguration<SalariesBotMessage>
{
    public void Configure(
        EntityTypeBuilder<SalariesBotMessage> builder)
    {
        builder.ToTable("SalariesBotMessages");
        builder.HasKey(x => x.Id);

        builder
            .Property(x => x.Username)
            .IsRequired(false);

        builder
            .Property(x => x.ChatId)
            .IsRequired(true);

        builder
            .HasIndex(x => x.ChatId)
            .IsUnique(true);
    }
}