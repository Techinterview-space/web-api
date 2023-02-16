using System;
using Domain.Entities.Organizations;
using Domain.Services.Users;

namespace Domain.Services.Organizations;

public record JoinToOrgInvitationDto
{
    public JoinToOrgInvitationDto()
    {
    }

    public JoinToOrgInvitationDto(
        JoinToOrgInvitation invitation)
    {
        Id = invitation.Id;
        OrganizationId = invitation.OrganizationId;
        OrganizationName = invitation.Organization?.Name;
        InvitedUserId = invitation.InvitedUserId;
        Status = invitation.Status;
        InviterId = invitation.InviterId;
        CreatedAt = invitation.CreatedAt;
        UpdatedAt = invitation.UpdatedAt;

        InvitedUser = UserDto.CreateFromEntityOrNull(invitation.InvitedUser);
        Inviter = UserDto.CreateFromEntityOrNull(invitation.Inviter);
    }

    public Guid Id { get; init; }

    public InvitationStatus Status { get; init; }

    public Guid OrganizationId { get; init; }

    public string OrganizationName { get; init; }

    public long InvitedUserId { get; init; }

    public UserDto InvitedUser { get; init; }

    public long InviterId { get; init; }

    public UserDto Inviter { get; init; }

    public DateTimeOffset CreatedAt { get; init; }

    public DateTimeOffset UpdatedAt { get; init; }
}