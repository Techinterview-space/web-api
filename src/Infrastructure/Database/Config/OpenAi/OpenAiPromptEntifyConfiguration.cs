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
            .Property(x => x.Type)
            .IsRequired();

        builder
            .Property(x => x.Prompt)
            .IsRequired();

        builder
            .Property(x => x.Model)
            .IsRequired();

        builder
            .Property(x => x.Engine)
            .IsRequired()
            .HasDefaultValue(AiEngine.OpenAi);

        builder.HasIndex(x => new
        {
            x.Type,
            x.IsActive
        });
    }
}