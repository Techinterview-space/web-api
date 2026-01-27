using System;
using Web.Api.Features.SalarySubscribtions.Shared;

namespace Web.Api.Features.SalarySubscribtions.EditSubscription;

public record EditSalarySubscriptionCommand : EditSalarySubscriptionBodyRequest
{
    public Guid SubscriptionId { get; }

    public EditSalarySubscriptionCommand(
        Guid subscriptionId,
        EditSalarySubscriptionBodyRequest request)
    {
        SubscriptionId = subscriptionId;
        Name = request.Name;
        ProfessionIds = request.ProfessionIds;
        PreventNotificationIfNoDifference = request.PreventNotificationIfNoDifference;
        Regularity = request.Regularity;
        UseAiAnalysis = request.UseAiAnalysis;
    }
}