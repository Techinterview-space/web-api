using Domain.Entities.StatData;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Database.Config;

public class StatDataChangeSubscriptionRecordEntityConfig : IEntityTypeConfiguration<StatDataChangeSubscriptionRecord>
{
    public void Configure(EntityTypeBuilder<StatDataChangeSubscriptionRecord> builder)
    {
        builder.ToTable($"{nameof(StatDataChangeSubscriptionRecord)}s");
        builder.HasKey(x => x.Id);

        builder
            .HasOne(x => x.StatDataChangeSubscription)
            .WithMany(x => x.StatDataChangeSubscriptionRecords)
            .HasForeignKey(x => x.StatDataChangeSubscriptionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .HasOne(x => x.PreviousStatDataChangeSubscriptionRecord)
            .WithMany(x => x.NextStatDataChangeSubscriptionRecords)
            .HasForeignKey(x => x.PreviousStatDataChangeSubscriptionRecordId)
            .OnDelete(DeleteBehavior.SetNull);

        builder
            .Property(x => x.Data)
            .HasJsonConversion();
    }
}