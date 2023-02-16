using Domain.Entities.Employments;
using Domain.Entities.Interviews;
using Domain.Entities.Labels;
using Domain.Entities.Organizations;
using Domain.Entities.Users;
using MG.Utils.EFCore;
using Microsoft.EntityFrameworkCore;

namespace Domain.Database;

public class DatabaseContext : AppDbContextBase<DatabaseContext>
{
    public DatabaseContext(DbContextOptions<DatabaseContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users { get; set; }

    public DbSet<InterviewTemplate> InterviewTemplates { get; set; }

    public DbSet<Interview> Interviews { get; set; }

    public DbSet<UserLabel> UserLabels { get; set; }

    public DbSet<OrganizationLabel> OrganizationLabels { get; set; }

    public DbSet<CandidateCard> CandidateCards { get; set; }

    public DbSet<Candidate> Candidates { get; set; }

    public DbSet<CandidateInterview> CandidateInterviews { get; set; }

    public DbSet<Organization> Organizations { get; set; }

    public DbSet<JoinToOrgInvitation> JoinToOrgInvitations { get; set; }

    public DbSet<OrganizationUser> OrganizationUsers { get; set; }
}