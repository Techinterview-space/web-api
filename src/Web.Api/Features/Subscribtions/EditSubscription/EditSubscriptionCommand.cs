using System;
using Web.Api.Features.Subscribtions.Shared;

namespace Web.Api.Features.Subscribtions.EditSubscription;

public record EditSubscriptionCommand : EditSubscriptionBodyRequest
{
    public Guid SubscriptionId { get; }

    public EditSubscriptionCommand(
        Guid subscriptionId,
        EditSubscriptionBodyRequest request)
    {
        SubscriptionId = subscriptionId;
        Name = request.Name;
        ProfessionIds = request.ProfessionIds;
        PreventNotificationIfNoDifference = request.PreventNotificationIfNoDifference;
        Regularity = request.Regularity;
        UseAiAnalysis = request.UseAiAnalysis;
    }
}