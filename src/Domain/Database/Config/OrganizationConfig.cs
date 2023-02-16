using Domain.Entities.Organizations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Domain.Database.Config;

public class OrganizationConfig
    : IEntityTypeConfiguration<Organization>,
        IEntityTypeConfiguration<OrganizationUser>,
        IEntityTypeConfiguration<JoinToOrgInvitation>
{
    public void Configure(EntityTypeBuilder<Organization> builder)
    {
        builder.ToTable("Organizations");
        builder
            .HasOne(x => x.Manager)
            .WithMany()
            .HasForeignKey(x => x.ManagerId);
    }

    public void Configure(EntityTypeBuilder<OrganizationUser> builder)
    {
        builder.ToTable("OrganizationUsers");

        builder
            .HasOne(x => x.User)
            .WithMany(x => x.OrganizationUsers)
            .HasForeignKey(x => x.UserId);

        builder
            .HasOne(x => x.Organization)
            .WithMany(x => x.Users)
            .HasForeignKey(x => x.OrganizationId);
    }

    public void Configure(EntityTypeBuilder<JoinToOrgInvitation> builder)
    {
        builder.ToTable("JoinToOrgInvitations");
        builder
            .HasOne(x => x.Organization)
            .WithMany(x => x.Invitations)
            .HasForeignKey(x => x.OrganizationId);

        builder
            .HasOne(x => x.InvitedUser)
            .WithMany()
            .HasForeignKey(x => x.InvitedUserId);

        builder
            .HasOne(x => x.Inviter)
            .WithMany()
            .HasForeignKey(x => x.InviterId);
    }
}