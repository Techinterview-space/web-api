using System;
using System.Collections.Generic;
using System.Linq;
using Domain.Entities.Surveys;
using Domain.Enums;

namespace Web.Api.Features.PublicSurveys.Dtos;

public record PublicSurveyDto
{
    public Guid Id { get; init; }

    public string Title { get; init; }

    public string Description { get; init; }

    public string Slug { get; init; }

    public long AuthorId { get; init; }

    public PublicSurveyStatus Status { get; init; }

    public DateTimeOffset CreatedAt { get; init; }

    public DateTimeOffset? PublishedAt { get; init; }

    public DateTimeOffset? DeletedAt { get; init; }

    public bool IsAuthor { get; init; }

    public List<PublicSurveyQuestionDto> Questions { get; init; }

    public PublicSurveyDto()
    {
    }

    public PublicSurveyDto(
        PublicSurvey survey,
        bool isAuthor,
        bool hasUserResponded = false)
    {
        Id = survey.Id;
        Title = survey.Title;
        Description = survey.Description;
        Slug = survey.Slug;
        AuthorId = survey.AuthorId;
        Status = survey.Status;
        CreatedAt = survey.CreatedAt;
        PublishedAt = survey.PublishedAt;
        DeletedAt = survey.DeletedAt;
        IsAuthor = isAuthor;

        Questions = survey.Questions?
            .OrderBy(q => q.Order)
            .Select(q => new PublicSurveyQuestionDto(q, hasUserResponded))
            .ToList() ?? new List<PublicSurveyQuestionDto>();
    }
}
