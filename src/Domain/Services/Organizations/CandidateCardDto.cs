using System;
using System.Collections.Generic;
using System.Linq;
using Domain.Entities.Employments;
using Domain.Services.Labels;
using Domain.Services.Users;

namespace Domain.Services.Organizations;

public record CandidateCardDto
{
    public CandidateCardDto()
    {
    }

    public CandidateCardDto(
        CandidateCard candidateCard,
        bool setCandidate = false)
    {
        Id = candidateCard.Id;
        EmploymentStatus = candidateCard.EmploymentStatus;
        CandidateId = candidateCard.CandidateId;
        Candidate = setCandidate && candidateCard.Candidate != null
            ? new CandidateDto(candidateCard.Candidate)
            : null;

        OrganizationId = candidateCard.OrganizationId;
        Organization = candidateCard.Organization != null
            ? new OrganizationSimpleDto(candidateCard.Organization)
            : null;

        OpenById = candidateCard.OpenById;
        OpenBy = candidateCard.OpenBy != null
            ? new UserDto(candidateCard.OpenBy)
            : null;

        Interviews = candidateCard.Interviews?.Select(x => new CandidateInterviewDto(x)) ?? Array.Empty<CandidateInterviewDto>();
        Comments = candidateCard.Comments?.Select(x => new CandidateCardCommentDto(x)) ?? Array.Empty<CandidateCardCommentDto>();
        Labels = candidateCard.Labels?.Select(x => new LabelDto(x)) ?? Array.Empty<LabelDto>();
        Files = candidateCard.Files ?? new List<CandidateCardCvFile>();

        CreatedAt = candidateCard.CreatedAt;
        UpdatedAt = candidateCard.UpdatedAt;
        DeletedAt = candidateCard.DeletedAt;
    }

    public CandidateCardDto(
        CandidateCard candidateCard,
        Candidate candidate,
        bool setCandidate = false)
        : this(candidateCard, setCandidate)
    {
        Candidate = candidate != null
            ? new CandidateDto(candidate)
            : null;
    }

    public Guid Id { get; }

    public EmploymentStatus EmploymentStatus { get; }

    public Guid CandidateId { get; }

    public CandidateDto Candidate { get; }

    public Guid OrganizationId { get; }

    public OrganizationSimpleDto Organization { get; }

    public long? OpenById { get; }

    public UserDto OpenBy { get; }

    public IEnumerable<CandidateInterviewDto> Interviews { get; }

    public IEnumerable<CandidateCardCommentDto> Comments { get; }

    public IEnumerable<LabelDto> Labels { get; }

    public IEnumerable<CandidateCardCvFile> Files { get; }

    public DateTimeOffset CreatedAt { get; }

    public DateTimeOffset UpdatedAt { get; }

    public DateTimeOffset? DeletedAt { get; }

    public bool Active => !DeletedAt.HasValue;
}