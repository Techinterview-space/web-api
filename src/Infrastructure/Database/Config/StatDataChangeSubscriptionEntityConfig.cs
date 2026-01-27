using Domain.Entities.StatData;
using Domain.Entities.StatData.Salary;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Infrastructure.Database.Config;

public class StatDataChangeSubscriptionEntityConfig : IEntityTypeConfiguration<StatDataChangeSubscription>
{
    public void Configure(
        EntityTypeBuilder<StatDataChangeSubscription> builder)
    {
        builder.ToTable($"{nameof(StatDataChangeSubscription)}s");
        builder.HasKey(x => x.Id);

        builder
            .Property(x => x.Name)
            .HasMaxLength(500)
            .IsRequired(true);

        builder
            .Property(x => x.ProfessionIds)
            .HasJsonConversion();

        builder
            .Property(x => x.PreventNotificationIfNoDifference)
            .IsRequired()
            .HasDefaultValue(false);

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