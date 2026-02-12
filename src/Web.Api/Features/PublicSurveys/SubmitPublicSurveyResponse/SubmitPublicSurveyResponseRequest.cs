using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Web.Api.Features.PublicSurveys.SubmitPublicSurveyResponse;

public record SubmitPublicSurveyResponseRequest
{
    [Required]
    [MinLength(1)]
    public List<Guid> OptionIds { get; init; }
}
