using Domain.Entities.Vacancies;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Database.Config;

public class VacancyEntityConfiguration : IEntityTypeConfiguration<Vacancy>
{
    public void Configure(
        EntityTypeBuilder<Vacancy> builder)
    {
        builder.ToTable("Vacancies");
        builder.HasKey(x => x.Id);

        builder
            .Property(x => x.Title)
            .IsRequired();

        builder
            .HasOne(x => x.Company)
            .WithMany(x => x.Vacancies)
            .HasForeignKey(x => x.CompanyId)
            .IsRequired(false);

        builder
            .HasOne(x => x.Author)
            .WithMany(x => x.Vacancies)
            .HasForeignKey(x => x.AuthorId);

        builder.HasIndex(x => x.Status);
        builder.HasIndex(x => x.CompanyId);
        builder.HasIndex(x => x.AuthorId);
        builder.HasIndex(x => x.CreatedAt);
    }
}
