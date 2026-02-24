using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Web.Api.Features.PublicSurveys.UpdatePublicSurvey;

public record UpdatePublicSurveyRequest
{
    [StringLength(500)]
    public string Title { get; init; }

    [StringLength(2000)]
    public string Description { get; init; }

    [StringLength(100)]
    public string Slug { get; init; }

    [MinLength(1)]
    [MaxLength(30)]
    public List<UpdatePublicSurveyQuestionRequest> Questions { get; init; }
}

public record UpdatePublicSurveyQuestionRequest
{
    [Required]
    [StringLength(500)]
    public string Text { get; init; }

    public int Order { get; init; }

    public bool AllowMultipleChoices { get; init; }

    [Required]
    [MinLength(2)]
    [MaxLength(10)]
    public List<UpdatePublicSurveyOptionRequest> Options { get; init; }
}

public record UpdatePublicSurveyOptionRequest
{
    [Required]
    [StringLength(200)]
    public string Text { get; init; }

    public int Order { get; init; }
}

public record UpdatePublicSurveyCommand
{
    public UpdatePublicSurveyCommand(
        Guid surveyId,
        UpdatePublicSurveyRequest body)
    {
        SurveyId = surveyId;
        Body = body;
    }

    public Guid SurveyId { get; }

    public UpdatePublicSurveyRequest Body { get; }
}
