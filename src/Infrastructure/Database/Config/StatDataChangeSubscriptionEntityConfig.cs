using Domain.Entities.StatData;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

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
    }
}