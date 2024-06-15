using System;
using System.Linq;
using Domain.Entities.Interviews;
using Web.Api.Features.Labels.Models;
using Web.Api.Features.Users.Models;

namespace Web.Api.Features.Interviews.Models;

public record InterviewTemplateDto : InterviewTemplateUpdateRequest
{
    public InterviewTemplateDto()
    {
    }

    public InterviewTemplateDto(
        InterviewTemplate interviewTemplate)
    {
        Id = interviewTemplate.Id;
        Title = interviewTemplate.Title;
        OverallOpinion = interviewTemplate.OverallOpinion;
        AuthorId = interviewTemplate.AuthorId;
        Author = UserDto.CreateFromEntityOrNull(interviewTemplate.Author);
        Subjects = interviewTemplate.Subjects;
        CreatedAt = interviewTemplate.CreatedAt;
        UpdatedAt = interviewTemplate.UpdatedAt;
        IsPublic = interviewTemplate.IsPublic;
        Labels = interviewTemplate.Labels.Select(x => new LabelDto(x)).ToList();
    }

    public long AuthorId { get; init; }

    public UserDto Author { get; init; }

    public DateTimeOffset CreatedAt { get; init; }

    public DateTimeOffset UpdatedAt { get; init; }
}