using System;
using System.Collections.Generic;
using System.Linq;
using Domain.Entities.Employments;
using Domain.Services.Users;

namespace Domain.Services.Organizations;

public record CandidateDto
{
    public CandidateDto()
    {
    }

    public CandidateDto(
        Candidate candidate,
        bool setOrganization = false)
    {
        Id = candidate.Id;
        FirstName = candidate.FirstName;
        LastName = candidate.LastName;
        Contacts = candidate.Contacts;
        CreatedById = candidate.CreatedById;
        CreatedBy = candidate.CreatedBy != null
            ? new UserDto(candidate.CreatedBy)
            : null;

        CreatedAt = candidate.CreatedAt;
        UpdatedAt = candidate.UpdatedAt;
        DeletedAt = candidate.DeletedAt;
        OrganizationId = candidate.OrganizationId;
        Organization = setOrganization && candidate.Organization != null
            ? new OrganizationDto(candidate.Organization)
            : null;

        CandidateCards = candidate.CandidateCards?.Select(x => new CandidateCardDto(x));
    }

    public Guid Id { get; init; }

    public string FirstName { get; init; }

    public string LastName { get; init; }

    public string Contacts { get; init; }

    public long? CreatedById { get; init; }

    public UserDto CreatedBy { get; init; }

    public Guid OrganizationId { get; init; }

    public OrganizationDto Organization { get; init; }

    public IEnumerable<CandidateCardDto> CandidateCards { get; init; }

    public DateTimeOffset CreatedAt { get; init; }

    public DateTimeOffset UpdatedAt { get; init; }

    public DateTimeOffset? DeletedAt { get; init; }

    public bool Active => !DeletedAt.HasValue;
}