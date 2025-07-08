using Domain.Entities.StatData;
using Domain.Entities.StatData.Salary;
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
            .HasOne(x => x.Subscription)
            .WithMany(x => x.Records)
            .HasForeignKey(x => x.SubscriptionId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("FK_StatDataChangeSubscriptionRecord_Subscription");

        builder
            .HasOne(x => x.PreviousRecord)
            .WithMany(x => x.NextRecords)
            .HasForeignKey(x => x.PreviousRecordId)
            .OnDelete(DeleteBehavior.SetNull)
            .HasConstraintName("FK_StatDataChangeSubscriptionRecord_PreviousRecord");

        builder
            .Property(x => x.Data)
            .HasJsonConversion();
    }
}