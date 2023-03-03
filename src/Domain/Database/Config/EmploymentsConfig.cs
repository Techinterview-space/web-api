using Domain.Entities.Employments;
using Domain.Services.Organizations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Domain.Database.Config;

public class EmploymentsConfig
    : IEntityTypeConfiguration<Candidate>,
        IEntityTypeConfiguration<CandidateCard>,
        IEntityTypeConfiguration<CandidateInterview>,
        IEntityTypeConfiguration<CandidateCardComment>
{
    public void Configure(EntityTypeBuilder<Candidate> builder)
    {
        builder.ToTable("Candidates");
        builder
            .Property(x => x.CvFiles)
            .HasJsonConversion();

        builder
            .HasOne(x => x.CreatedBy)
            .WithMany()
            .HasForeignKey(x => x.CreatedById);

        builder
            .HasOne(x => x.Organization)
            .WithMany(x => x.Candidates)
            .HasForeignKey(x => x.OrganizationId);
    }

    public void Configure(EntityTypeBuilder<CandidateCard> builder)
    {
        builder.ToTable("CandidateCards");
        builder
            .HasOne(x => x.Candidate)
            .WithMany(x => x.CandidateCards)
            .HasForeignKey(x => x.CandidateId);

        builder
            .HasOne(x => x.OpenBy)
            .WithMany()
            .HasForeignKey(x => x.OpenById);

        builder
            .HasOne(x => x.Organization)
            .WithMany(x => x.CandidateCards)
            .HasForeignKey(x => x.OrganizationId);

        builder
            .HasMany(x => x.Labels)
            .WithMany(x => x.Cards);

        builder
            .Property(x => x.Files)
            .HasJsonConversion();
    }

    public void Configure(EntityTypeBuilder<CandidateInterview> builder)
    {
        builder.ToTable("CandidateInterviews");
        builder
            .HasOne(x => x.CandidateCard)
            .WithMany(x => x.Interviews)
            .HasForeignKey(x => x.CandidateCardId);

        builder
            .HasOne(x => x.Interview)
            .WithOne(x => x.CandidateInterview)
            .HasForeignKey<CandidateInterview>(x => x.InterviewId);

        builder
            .HasOne(x => x.OrganizedBy)
            .WithMany()
            .HasForeignKey(x => x.OrganizedById);
    }

    public void Configure(EntityTypeBuilder<CandidateCardComment> builder)
    {
        builder.ToTable("CandidateCardComments");
        builder
            .HasOne(x => x.CandidateCard)
            .WithMany(x => x.Comments)
            .HasForeignKey(x => x.CandidateCardId);

        builder
            .HasOne(x => x.Author)
            .WithMany()
            .HasForeignKey(x => x.AuthorId);
    }
}