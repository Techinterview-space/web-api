using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Web.Api.Features.PublicSurveys.CreatePublicSurvey;

public record CreatePublicSurveyRequest
{
    [Required]
    [StringLength(500)]
    public string Title { get; init; }

    [StringLength(2000)]
    public string Description { get; init; }

    [Required]
    [StringLength(100)]
    public string Slug { get; init; }

    [Required]
    [MinLength(1)]
    [MaxLength(30)]
    public List<CreatePublicSurveyQuestionRequest> Questions { get; init; }
}

public record CreatePublicSurveyQuestionRequest
{
    [Required]
    [StringLength(500)]
    public string Text { get; init; }

    public int Order { get; init; }

    public bool AllowMultipleChoices { get; init; }

    [Required]
    [MinLength(2)]
    [MaxLength(10)]
    public List<string> Options { get; init; }
}
