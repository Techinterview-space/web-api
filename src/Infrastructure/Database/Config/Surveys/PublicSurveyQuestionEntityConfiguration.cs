using Domain.Entities.Surveys;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Database.Config.Surveys;

public class PublicSurveyQuestionEntityConfiguration : IEntityTypeConfiguration<PublicSurveyQuestion>
{
    public void Configure(
        EntityTypeBuilder<PublicSurveyQuestion> builder)
    {
        builder.HasKey(x => x.Id);

        builder
            .Property(x => x.Text)
            .HasMaxLength(PublicSurveyQuestion.TextMaxLength)
            .IsRequired();

        builder
            .HasIndex(x => new { x.PublicSurveyId, x.Order })
            .IsUnique();

        builder
            .HasOne(x => x.PublicSurvey)
            .WithMany(x => x.Questions)
            .HasForeignKey(x => x.PublicSurveyId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
