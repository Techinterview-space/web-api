using Domain.Entities.Surveys;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Database.Config.Surveys;

public class PublicSurveyEntityConfiguration : IEntityTypeConfiguration<PublicSurvey>
{
    public void Configure(
        EntityTypeBuilder<PublicSurvey> builder)
    {
        builder.HasKey(x => x.Id);

        builder
            .Property(x => x.Id)
            .ValueGeneratedNever();

        builder
            .Property(x => x.Title)
            .HasMaxLength(PublicSurvey.TitleMaxLength)
            .IsRequired();

        builder
            .Property(x => x.Description)
            .HasMaxLength(PublicSurvey.DescriptionMaxLength);

        builder
            .Property(x => x.Slug)
            .HasMaxLength(PublicSurvey.SlugMaxLength)
            .IsRequired();

        builder
            .HasIndex(x => x.Slug)
            .IsUnique()
            .HasFilter("\"DeletedAt\" IS NULL");

        builder
            .HasOne(x => x.Author)
            .WithMany()
            .HasForeignKey(x => x.AuthorId)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .HasIndex(x => x.AuthorId);

        builder
            .HasIndex(x => x.Status);
    }
}
