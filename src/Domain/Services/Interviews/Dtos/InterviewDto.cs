using System;
using System.Linq;
using Domain.Entities.Interviews;
using Domain.Services.Labels;
using Domain.Services.Users;

namespace Domain.Services.Interviews.Dtos;

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
        CreatedAt = interview.CreatedAt;
        UpdatedAt = interview.UpdatedAt;
        Labels = interview.Labels.Select(x => new LabelDto(x)).ToList();
    }

    public long InterviewerId { get; init; }

    public UserDto Interviewer { get; init; }

    public DateTimeOffset CreatedAt { get; init; }

    public DateTimeOffset UpdatedAt { get; init; }
}