using Domain.Entities.Surveys;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Database.Config.Surveys;

public class PublicSurveyOptionEntityConfiguration : IEntityTypeConfiguration<PublicSurveyOption>
{
    public void Configure(
        EntityTypeBuilder<PublicSurveyOption> builder)
    {
        builder.HasKey(x => x.Id);

        builder
            .Property(x => x.Text)
            .HasMaxLength(PublicSurveyOption.TextMaxLength)
            .IsRequired();

        builder
            .HasIndex(x => new { x.QuestionId, x.Order })
            .IsUnique();

        builder
            .HasOne(x => x.Question)
            .WithMany(x => x.Options)
            .HasForeignKey(x => x.QuestionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
