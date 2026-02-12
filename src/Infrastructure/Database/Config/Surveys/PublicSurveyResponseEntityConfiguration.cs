using Domain.Entities.Surveys;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Database.Config.Surveys;

public class PublicSurveyResponseEntityConfiguration : IEntityTypeConfiguration<PublicSurveyResponse>
{
    public void Configure(
        EntityTypeBuilder<PublicSurveyResponse> builder)
    {
        builder.HasKey(x => x.Id);

        builder
            .HasIndex(x => new { x.QuestionId, x.UserId })
            .IsUnique();

        builder
            .HasOne(x => x.Question)
            .WithMany(x => x.Responses)
            .HasForeignKey(x => x.QuestionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
