using Domain.Entities.Telegram;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Database.Config;

public class TelegramInlineReplyEntityConfiguration
    : IEntityTypeConfiguration<TelegramInlineReply>
{
    public void Configure(
        EntityTypeBuilder<TelegramInlineReply> builder)
    {
        builder.ToTable("TelegramInlineReplies");
        builder.HasKey(x => x.Id);
    }
}