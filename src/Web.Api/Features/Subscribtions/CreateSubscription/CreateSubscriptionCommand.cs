using MediatR;
using Web.Api.Features.Subscribtions.GetStatDataChangeSubscriptions;

namespace Web.Api.Features.Subscribtions.CreateSubscription;

public record CreateSubscriptionCommand
    : CreateSubscriptionBodyRequest, IRequest<StatDataChangeSubscriptionDto>
{
    public CreateSubscriptionCommand(
        CreateSubscriptionBodyRequest request)
    {
        Name = request.Name;
        TelegramChatId = request.TelegramChatId;
        ProfessionIds = request.ProfessionIds;
        PreventNotificationIfNoDifference = request.PreventNotificationIfNoDifference;
    }
}