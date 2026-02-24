using System;
using System.Collections.Generic;

namespace Web.Api.Features.PublicSurveys.Dtos;

public record PublicSurveyResultsDto
{
    public int TotalResponses { get; init; }

    public List<PublicSurveyQuestionResultDto> Questions { get; init; }
}

public record PublicSurveyQuestionResultDto
{
    public Guid Id { get; init; }

    public string Text { get; init; }

    public int Order { get; init; }

    public List<PublicSurveyOptionResultDto> Options { get; init; }
}

public record PublicSurveyOptionResultDto
{
    public Guid Id { get; init; }

    public string Text { get; init; }

    public int ResponseCount { get; init; }

    public decimal Percentage { get; init; }
}
