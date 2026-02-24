using System;
using System.Collections.Generic;
using System.Linq;
using Domain.Entities.Surveys;

namespace Web.Api.Features.PublicSurveys.Dtos;

public record PublicSurveyQuestionDto
{
    public Guid Id { get; init; }

    public string Text { get; init; }

    public int Order { get; init; }

    public bool AllowMultipleChoices { get; init; }

    public List<PublicSurveyOptionDto> Options { get; init; }

    public bool HasUserResponded { get; init; }

    public int TotalResponses { get; init; }

    public PublicSurveyQuestionDto()
    {
    }

    public PublicSurveyQuestionDto(
        PublicSurveyQuestion question,
        bool hasUserResponded = false)
    {
        Id = question.Id;
        Text = question.Text;
        Order = question.Order;
        AllowMultipleChoices = question.AllowMultipleChoices;
        Options = question.Options?
            .OrderBy(o => o.Order)
            .Select(o => new PublicSurveyOptionDto(o))
            .ToList() ?? new List<PublicSurveyOptionDto>();
        HasUserResponded = hasUserResponded;
        TotalResponses = question.Responses?.Count ?? 0;
    }
}
