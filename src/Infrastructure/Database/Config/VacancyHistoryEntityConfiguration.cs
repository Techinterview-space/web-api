using Domain.Entities.Vacancies;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Database.Config;

public class VacancyHistoryEntityConfiguration : IEntityTypeConfiguration<VacancyHistory>
{
    public void Configure(
        EntityTypeBuilder<VacancyHistory> builder)
    {
        builder.ToTable("VacancyHistory");
        builder.HasKey(x => x.Id);

        builder
            .Property(x => x.Message)
            .IsRequired();

        builder
            .Property(x => x.CreatedBy)
            .HasMaxLength(320);

        builder
            .HasOne(x => x.Vacancy)
            .WithMany(x => x.History)
            .HasForeignKey(x => x.VacancyId);

        builder.HasIndex(x => x.VacancyId);
    }
}
