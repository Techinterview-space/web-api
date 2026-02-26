using Domain.Entities.ChannelStats;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Database.Config.ChannelStats;

internal class TelegramRawUpdateDbConfig : IEntityTypeConfiguration<TelegramRawUpdate>
{
    public void Configure(EntityTypeBuilder<TelegramRawUpdate> builder)
    {
        builder.HasIndex(x => x.UpdateId).IsUnique();

        builder.HasIndex(x => x.Status);

        builder
            .Property(x => x.PayloadJson)
            .IsRequired();

        builder
            .Property(x => x.Error)
            .HasMaxLength(2000);
    }
}
