using Domain.Entities.HistoricalRecords;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Database.Config.HistoricalRecords;

public class SalariesHistoricalDataRecordTemplateEntityConfiguration : IEntityTypeConfiguration<SalariesHistoricalDataRecordTemplate>
{
    public void Configure(
        EntityTypeBuilder<SalariesHistoricalDataRecordTemplate> builder)
    {
        builder.ToTable("SalariesHistoricalDataRecordTemplates");
        builder.HasKey(x => x.Id);

        builder
            .Property(x => x.Name)
            .IsRequired();

        builder
            .Property(x => x.ProfessionIds)
            .HasJsonConversion();
    }
}