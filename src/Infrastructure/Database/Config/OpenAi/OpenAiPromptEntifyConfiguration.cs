using Domain.Entities.OpenAI;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Database.Config.OpenAi;

public class OpenAiPromptEntifyConfiguration : IEntityTypeConfiguration<OpenAiPrompt>
{
    public void Configure(
        EntityTypeBuilder<OpenAiPrompt> builder)
    {
        builder.ToTable("OpenAiPrompts");
        builder.HasKey(x => x.Id);

        builder
            .Property(x => x.Prompt)
            .IsRequired();

        var createdAt = new DateTimeOffset(
            new DateTime(
                2025,
                6,
                29,
                12,
                4,
                0,
                0,
                DateTimeKind.Utc),
            TimeSpan.Zero);

        builder.HasData(
            new OpenAiPrompt(
                OpenAiPromptType.Company,
                OpenAiPrompt.DefaultCompanyAnalyzePrompt,
                createdAt),
            new OpenAiPrompt(
                OpenAiPromptType.Chat,
                OpenAiPrompt.DefaultChatAnalyzePrompt,
                createdAt));
    }
}