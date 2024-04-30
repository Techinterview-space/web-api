using Domain.Entities.Questions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Database.Config;

public class SalariesSurveyQuestionConfig : IEntityTypeConfiguration<SalariesSurveyQuestion>
{
    public void Configure(
        EntityTypeBuilder<SalariesSurveyQuestion> builder)
    {
        builder.ToTable("SalariesSurveyQuestions");

        builder
            .HasOne(x => x.CreatedByUser)
            .WithMany()
            .HasForeignKey(x => x.CreatedByUserId);

        builder.HasData(
            new SalariesSurveyQuestion(
                Guid.Parse("703dc7ad-1395-4c09-bb76-a08b639134e4"),
                "Ожидали ли вы, что медианная и средняя зарплаты будут такими, какими они оказались?",
                null,
                DateTime.Parse("2024-04-30T00:00:00Z")));
    }
}