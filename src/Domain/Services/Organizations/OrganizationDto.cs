using System;
using System.Collections.Generic;
using System.Linq;
using Domain.Entities.Organizations;
using Domain.Services.Users;

namespace Domain.Services.Organizations;

public record OrganizationDto : OrganizationSimpleDto
{
    public OrganizationDto()
    {
    }

    public OrganizationDto(
        Organization organization)
        : base(organization)
    {
        Description = organization.Description;
        Manager = organization.Manager != null
            ? new UserDto(organization.Manager)
            : null;

        CreatedAt = organization.CreatedAt;
        UpdatedAt = organization.UpdatedAt;
        DeletedAt = organization.DeletedAt;
        Users = organization.Users?.Select(x => new OrganizationUserDto(x, true)) ?? Array.Empty<OrganizationUserDto>();
        Candidates = organization.Candidates?.Select(x => new CandidateDto(x)) ?? Array.Empty<CandidateDto>();
        Invitations = organization.Invitations?.Select(x => new JoinToOrgInvitationDto(x)) ?? Array.Empty<JoinToOrgInvitationDto>();
        CandidateCards = organization.CandidateCards?.Select(x => new CandidateCardDto(x)) ?? Array.Empty<CandidateCardDto>();
    }

    public string Description { get; }

    public UserDto Manager { get; }

    public IEnumerable<OrganizationUserDto> Users { get; }

    public IEnumerable<CandidateDto> Candidates { get; }

    public IEnumerable<JoinToOrgInvitationDto> Invitations { get; }

    public IEnumerable<CandidateCardDto> CandidateCards { get; }

    public DateTimeOffset CreatedAt { get; }

    public DateTimeOffset UpdatedAt { get; }

    public DateTimeOffset? DeletedAt { get; }

    public bool Active => DeletedAt == null;
}
