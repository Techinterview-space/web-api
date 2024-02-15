using Domain.Entities.Salaries;
using Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Domain.Database.Config;

public class WorkIndustryConfig : IEntityTypeConfiguration<WorkIndustry>
{
    public void Configure(EntityTypeBuilder<WorkIndustry> builder)
    {
        builder.ToTable("WorkIndustries");
        builder
            .Property(x => x.HexColor)
            .HasConversion(
                x => x.ToString(),
                x => new HexColor(x));

        builder
            .HasOne(x => x.CreatedBy)
            .WithMany()
            .HasForeignKey(x => x.CreatedById);
    }
}