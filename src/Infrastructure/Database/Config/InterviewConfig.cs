using Domain.Entities.Interviews;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Database.Config;

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
            .Property(x => x.Subjects)
            .HasJsonConversion();

        builder
            .HasOne(x => x.ShareLink)
            .WithOne(isl => isl.Interview)
            .HasForeignKey<ShareLink>(isl => isl.InterviewId);

        builder
            .HasMany(x => x.Labels)
            .WithMany(x => x.Interviews);
    }
}