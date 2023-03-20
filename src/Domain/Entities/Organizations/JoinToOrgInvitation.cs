using System;
using Domain.Entities.Users;

namespace Domain.Entities.Organizations;

public class JoinToOrgInvitation : HasDatesBase, IHasIdBase<Guid>
{
    protected JoinToOrgInvitation()
    {
    }

    public JoinToOrgInvitation(
        Organization organization,
        User invitedUser,
        User inviter)
    {
        Id = Guid.NewGuid();
        OrganizationId = organization.Id;
        Status = InvitationStatus.Pending;
        InvitedUserId = invitedUser.Id;
        InviterId = inviter.Id;

        if (InviterId == InvitedUserId)
        {
            throw new ArgumentException("Inviter and invited user cannot be the same");
        }
    }

    public Guid Id { get; protected set; }

    public InvitationStatus Status { get; protected set; }

    public Guid OrganizationId { get; protected set; }

    public virtual Organization Organization { get; protected set; }

    public long InvitedUserId { get; protected set; }

    public virtual User InvitedUser { get; protected set; }

    public long InviterId { get; protected set; }

    public virtual User Inviter { get; protected set; }

    public void Accept()
    {
        if (Organization is null)
        {
            throw new InvalidOperationException("Organization is null");
        }

        if (InvitedUser is null)
        {
            throw new InvalidOperationException("Organization is null");
        }

        if (Status is InvitationStatus.Accepted or InvitationStatus.Declined)
        {
            throw new InvalidOperationException($"Invitation is already {Status}");
        }

        Status = InvitationStatus.Accepted;
        Organization.Users.Add(new OrganizationUser(InvitedUser, Organization));
    }

    public void Decline()
    {
        if (Status is InvitationStatus.Accepted or InvitationStatus.Declined)
        {
            throw new InvalidOperationException($"Invitation is already {Status}");
        }

        Status = InvitationStatus.Declined;
    }

    public bool IsInviter(
        User user) =>
        user.Id == InviterId;

    public bool IsInvitedPerson(
        User user) =>
        user.Id == InvitedUserId;
}