using Domain.Entities.StatData;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Database.Config;

public class SubscriptionTelegramMessageEntityConfiguration : IEntityTypeConfiguration<SubscriptionTelegramMessage>
{
    public void Configure(
        EntityTypeBuilder<SubscriptionTelegramMessage> builder)
    {
        // TODO mgorbatyuk: rename table to SubscriptionTelegramMessages
        builder.ToTable("StatDataChangeSubscriptionTgMessages");
        builder.HasKey(x => x.Id);

        builder
            .HasOne(x => x.SalarySubscription)
            .WithMany(x => x.StatDataChangeSubscriptionTgMessages)
            .HasForeignKey(x => x.SalarySubscriptionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .HasOne(x => x.CompanyReviewsSubscription)
            .WithMany(x => x.TelegramMessages)
            .HasForeignKey(x => x.CompanyReviewsSubscriptionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}