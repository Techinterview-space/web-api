using System;
using Domain.Entities.Employments;
using Domain.Services.Interviews.Dtos;
using Domain.Services.Users;

namespace Domain.Services.Organizations;

public record CandidateInterviewDto
{
    public CandidateInterviewDto()
    {
    }

    public CandidateInterviewDto(
        CandidateInterview interview,
        bool setEntities = false)
    {
        Id = interview.Id;
        ConductedDuringStatus = interview.ConductedDuringStatus;
        Comments = interview.Comments;
        CandidateCardId = interview.CandidateCardId;
        OrganizationId = interview.CandidateCard?.OrganizationId;
        CandidateCard = setEntities && interview.CandidateCard != null
            ? new CandidateCardDto(interview.CandidateCard)
            : null;

        CandidateName = interview.CandidateCard?.Candidate?.Fullname;

        InterviewId = interview.InterviewId;
        Interview = setEntities && interview.Interview != null
            ? new InterviewDto(interview.Interview)
            : null;

        InterviewerName = interview.Interview?.Interviewer?.Fullname;
        InterviewerId = interview.Interview?.InterviewerId;

        OrganizedById = interview.OrganizedById;
        OrganizedBy = interview.OrganizedBy != null
            ? new UserDto(interview.OrganizedBy)
            : null;

        CreatedAt = interview.CreatedAt;
        UpdatedAt = interview.UpdatedAt;
    }

    public Guid Id { get; }

    public EmploymentStatus? ConductedDuringStatus { get; }

    public string Comments { get; }

    public Guid CandidateCardId { get; }

    public Guid? OrganizationId { get; }

    public CandidateCardDto CandidateCard { get; }

    public Guid? InterviewId { get; }

    public InterviewDto Interview { get; }

    public string InterviewerName { get; }

    public long? InterviewerId { get; }

    public string CandidateName { get; }

    public long? OrganizedById { get; }

    public UserDto OrganizedBy { get; }

    public DateTimeOffset CreatedAt { get; }

    public DateTimeOffset UpdatedAt { get; }

    public static CandidateInterviewDto CreateFromEntityOrNull(
        CandidateInterview interview)
    {
        return interview is not null
            ? new CandidateInterviewDto(interview, false)
            : null;
    }
}