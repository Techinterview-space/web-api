using Domain.Entities.Interviews;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Database.Config;

internal class InterviewTemplateConfig : IEntityTypeConfiguration<InterviewTemplate>
{
    public void Configure(EntityTypeBuilder<InterviewTemplate> builder)
    {
        builder.ToTable("InterviewTemplates");
        builder.HasKey(x => x.Id);

        builder
            .HasOne(x => x.Author)
            .WithMany()
            .HasForeignKey(x => x.AuthorId);

        builder
            .Property(x => x.Subjects)
            .HasJsonConversion();

        builder
            .HasMany(x => x.Labels)
            .WithMany(x => x.InterviewTemplates);
    }
}