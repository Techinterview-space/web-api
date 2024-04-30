using Domain.Entities.Questions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Database.Config;

public class SalariesSurveyReplyConfig : IEntityTypeConfiguration<SalariesSurveyReply>
{
    public void Configure(
        EntityTypeBuilder<SalariesSurveyReply> builder)
    {
        builder.ToTable("SalariesSurveyReplies");

        builder
            .HasOne(x => x.SalariesSurveyQuestion)
            .WithMany(x => x.Replies)
            .HasForeignKey(x => x.SalariesSurveyQuestionId);

        builder
            .HasOne(x => x.CreatedByUser)
            .WithMany()
            .HasForeignKey(x => x.CreatedByUserId);
    }
}