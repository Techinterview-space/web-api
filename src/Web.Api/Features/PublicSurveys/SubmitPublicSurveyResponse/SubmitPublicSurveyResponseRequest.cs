using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Web.Api.Features.PublicSurveys.SubmitPublicSurveyResponse;

public record SubmitPublicSurveyResponseRequest
{
    [Required]
    [MinLength(1)]
    public List<SubmitPublicSurveyAnswerRequest> Answers { get; init; }
}

public record SubmitPublicSurveyAnswerRequest
{
    [Required]
    public Guid QuestionId { get; init; }

    [Required]
    [MinLength(1)]
    public List<Guid> OptionIds { get; init; }
}
