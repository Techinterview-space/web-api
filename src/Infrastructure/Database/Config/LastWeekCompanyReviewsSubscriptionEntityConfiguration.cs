using Domain.Entities.StatData;
using Domain.Entities.StatData.CompanyReviews;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Infrastructure.Database.Config;

public class LastWeekCompanyReviewsSubscriptionEntityConfiguration
    : IEntityTypeConfiguration<LastWeekCompanyReviewsSubscription>
{
    public void Configure(
        EntityTypeBuilder<LastWeekCompanyReviewsSubscription> builder)
    {
        builder.ToTable("LastWeekCompanyReviewsSubscriptions");
        builder.HasKey(x => x.Id);

        builder
            .Property(x => x.Name)
            .HasMaxLength(500)
            .IsRequired(true);

        builder
            .Property(x => x.UseAiAnalysis)
            .IsRequired()
            .HasDefaultValue(false);

        builder
            .Property(x => x.Regularity)
            .IsRequired()
            .HasConversion<EnumToStringConverter<SubscriptionRegularityType>>()
            .HasDefaultValue(SubscriptionRegularityType.Weekly);
    }
}