using System;
using System.Linq;
using Domain.Entities.Interviews;
using TechInterviewer.Features.Labels.Models;
using TechInterviewer.Features.Users.Models;

namespace TechInterviewer.Features.Interviews.Models;

public record InterviewDto : InterviewUpdateRequest
{
    public InterviewDto()
    {
    }

    public InterviewDto(
        Interview interview)
    {
        Id = interview.Id;
        InterviewerId = interview.InterviewerId;
        CandidateName = interview.CandidateName;
        CandidateGrade = interview.CandidateGrade;
        OverallOpinion = interview.OverallOpinion;
        Interviewer = UserDto.CreateFromEntityOrNull(interview.Interviewer);
        Subjects = interview.Subjects;
        ShareToken = interview.ShareLink?.ShareToken;
        CreatedAt = interview.CreatedAt;
        UpdatedAt = interview.UpdatedAt;
        Labels = interview.Labels.Select(x => new LabelDto(x)).ToList();
    }

    public long InterviewerId { get; init; }

    public UserDto Interviewer { get; init; }
    public Guid? ShareToken { get; init; }

    public DateTimeOffset CreatedAt { get; init; }

    public DateTimeOffset UpdatedAt { get; init; }
}