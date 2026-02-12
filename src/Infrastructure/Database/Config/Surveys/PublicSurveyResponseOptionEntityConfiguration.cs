using Domain.Entities.Surveys;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Database.Config.Surveys;

public class PublicSurveyResponseOptionEntityConfiguration : IEntityTypeConfiguration<PublicSurveyResponseOption>
{
    public void Configure(
        EntityTypeBuilder<PublicSurveyResponseOption> builder)
    {
        builder
            .HasKey(x => new { x.ResponseId, x.OptionId });

        builder
            .HasOne(x => x.Response)
            .WithMany(x => x.SelectedOptions)
            .HasForeignKey(x => x.ResponseId)
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .HasOne(x => x.Option)
            .WithMany(x => x.ResponseOptions)
            .HasForeignKey(x => x.OptionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
