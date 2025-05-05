using Domain.Entities.StatData;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Database.Config;

public class AiAnalysisSubscriptionRecordEntityConfig : IEntityTypeConfiguration<AiAnalysisSubscriptionRecord>
{
    public void Configure(
        EntityTypeBuilder<AiAnalysisSubscriptionRecord> builder)
    {
        builder.ToTable("AiAnalysisSubscriptionRecords");
        builder.HasKey(x => x.Id);

        builder
            .Property(x => x.AiReport)
            .IsRequired();

        builder
            .Property(x => x.AitReportSource)
            .IsRequired();

        builder
            .HasOne(x => x.Subscription)
            .WithMany(x => x.AiAnalysisSubscriptionRecords)
            .HasForeignKey(x => x.SubscriptionId);
    }
}