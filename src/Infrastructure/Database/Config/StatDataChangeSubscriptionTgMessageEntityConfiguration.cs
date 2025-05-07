using Domain.Entities.StatData;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Database.Config;

public class StatDataChangeSubscriptionTgMessageEntityConfiguration : IEntityTypeConfiguration<StatDataChangeSubscriptionTgMessage>
{
    public void Configure(
        EntityTypeBuilder<StatDataChangeSubscriptionTgMessage> builder)
    {
        builder.ToTable($"{nameof(StatDataChangeSubscriptionTgMessage)}s");
        builder.HasKey(x => x.Id);

        builder
            .HasOne(x => x.Subscription)
            .WithMany(x => x.StatDataChangeSubscriptionTgMessages)
            .HasForeignKey(x => x.SubscriptionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}