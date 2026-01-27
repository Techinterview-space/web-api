using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Domain.Attributes;
using Domain.Entities.StatData;

namespace Web.Api.Features.CompanyReviewsSubscriptions.Shared;

public record EditCompanyReviewsSubscriptionBodyRequest
{
    [Required]
    public string Name { get; init; }

    [NotDefaultValue]
    public SubscriptionRegularityType Regularity { get; init; }

    public bool UseAiAnalysis { get; init; }
}