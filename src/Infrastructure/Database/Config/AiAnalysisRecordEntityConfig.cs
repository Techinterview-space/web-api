using Domain.Entities.StatData;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Database.Config;

public class AiAnalysisRecordEntityConfig : IEntityTypeConfiguration<AiAnalysisRecord>
{
    public void Configure(
        EntityTypeBuilder<AiAnalysisRecord> builder)
    {
        builder.ToTable("AiAnalysisRecords");
        builder.HasKey(x => x.Id);

        builder
            .Property(x => x.AiReport)
            .IsRequired();

        builder
            .Property(x => x.AitReportSource)
            .IsRequired();

        builder
            .HasOne(x => x.Subscription)
            .WithMany(x => x.AiAnalysisRecords)
            .HasForeignKey(x => x.SubscriptionId);
    }
}