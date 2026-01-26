using Domain.Entities.StatData;
using Domain.Entities.StatData.Salary;
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
            .Property(x => x.AiReportSource)
            .IsRequired();

        builder
            .HasOne(x => x.SalarySubscription)
            .WithMany(x => x.AiAnalysisRecords)
            .HasForeignKey(x => x.SubscriptionId);

        builder
            .HasOne(x => x.CompanyReviewsSubscription)
            .WithMany(x => x.AiAnalysisRecords)
            .HasForeignKey(x => x.CompanyReviewsSubscriptionId);
    }
}