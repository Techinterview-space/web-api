using System;
using System.Linq;
using Domain.Entities.Surveys;
using Domain.Enums;

namespace Web.Api.Features.PublicSurveys.Dtos;

public record MySurveyListItemDto
{
    public Guid Id { get; init; }

    public string Title { get; init; }

    public string Slug { get; init; }

    public PublicSurveyStatus Status { get; init; }

    public DateTimeOffset CreatedAt { get; init; }

    public DateTimeOffset? PublishedAt { get; init; }

    public DateTimeOffset? DeletedAt { get; init; }

    public int TotalResponses { get; init; }

    public int QuestionCount { get; init; }

    public MySurveyListItemDto()
    {
    }

    public MySurveyListItemDto(PublicSurvey survey)
    {
        Id = survey.Id;
        Title = survey.Title;
        Slug = survey.Slug;
        Status = survey.Status;
        CreatedAt = survey.CreatedAt;
        PublishedAt = survey.PublishedAt;
        DeletedAt = survey.DeletedAt;
        QuestionCount = survey.Questions?.Count ?? 0;
        TotalResponses = survey.Questions?
            .SelectMany(q => q.Responses ?? Enumerable.Empty<PublicSurveyResponse>())
            .Count() ?? 0;
    }
}
