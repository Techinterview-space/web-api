using System.ComponentModel.DataAnnotations;
using Domain.Attributes;
using Web.Api.Features.Subscribtions.Shared;

namespace Web.Api.Features.Subscribtions.CreateSubscription;

public record CreateSubscriptionBodyRequest : EditSubscriptionBodyRequest
{
    [Required]
    [NotDefaultValue]
    public long TelegramChatId { get; init; }
}