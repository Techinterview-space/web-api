using Domain.Entities.Interviews;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Domain.Database.Config;

internal class InterviewConfig : IEntityTypeConfiguration<Interview>
{
    public void Configure(EntityTypeBuilder<Interview> builder)
    {
        builder.ToTable("Interviews");
        builder.HasKey(x => x.Id);

        builder
            .HasOne(x => x.Interviewer)
            .WithMany()
            .HasForeignKey(x => x.InterviewerId);

        builder
            .HasOne(x => x.Organization)
            .WithMany(x => x.Interviews)
            .HasForeignKey(x => x.OrganizationId);

        builder
            .Property(x => x.Subjects)
            .HasJsonConversion();

        builder
            .HasMany(x => x.Labels)
            .WithMany(x => x.Interviews);
    }
}