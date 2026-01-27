using Domain.Entities.HistoricalRecords;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Database.Config.HistoricalRecords;

public class SalariesHistoricalDataRecordEntityConfiguration
    : IEntityTypeConfiguration<SalariesHistoricalDataRecord>
{
    public void Configure(
        EntityTypeBuilder<SalariesHistoricalDataRecord> builder)
    {
        builder.ToTable("SalariesHistoricalDataRecords");
        builder.HasKey(x => x.Id);

        builder
            .Property(x => x.Data)
            .HasJsonConversion();

        builder
            .HasOne(x => x.SalariesHistoricalDataRecordTemplate)
            .WithMany(x => x.SalariesHistoricalDataRecords)
            .HasForeignKey(x => x.TemplateId)
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .HasIndex(x => x.Date)
            .IsUnique(false);

        builder
            .HasIndex(x => x.TemplateId);
    }
}