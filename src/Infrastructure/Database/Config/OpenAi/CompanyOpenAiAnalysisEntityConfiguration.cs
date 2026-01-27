using Domain.Entities.Companies;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Database.Config.OpenAi;

public class CompanyOpenAiAnalysisEntityConfiguration : IEntityTypeConfiguration<CompanyOpenAiAnalysis>
{
    public void Configure(EntityTypeBuilder<CompanyOpenAiAnalysis> builder)
    {
        builder.ToTable("CompanyOpenAiAnalysisRecords");
        builder.HasKey(x => x.Id);

        builder
            .Property(x => x.AnalysisText)
            .IsRequired();

        builder
            .HasOne(x => x.Company)
            .WithMany(x => x.OpenAiAnalysisRecords)
            .HasForeignKey(x => x.CompanyId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}