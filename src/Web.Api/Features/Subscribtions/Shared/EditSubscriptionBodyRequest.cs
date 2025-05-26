using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Domain.Attributes;
using Domain.Entities.StatData;

namespace Web.Api.Features.Subscribtions.Shared;

public record EditSubscriptionBodyRequest
{
    [Required]
    public string Name { get; init; }

    public List<long> ProfessionIds { get; init; } = new List<long>();

    public bool PreventNotificationIfNoDifference { get; init; }

    [NotDefaultValue]
    public SubscriptionRegularityType Regularity { get; init; }

    public bool UseAiAnalysis { get; init; }
}