using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Web.Api.Features.Subscribtions.CreateSubscription;

public record CreateSubscriptionBodyRequest
{
    [Required]
    public string Name { get; init; }

    [Required]
    public long TelegramChatId { get; init; }

    public List<long> ProfessionIds { get; init; } = new List<long>();

    public bool PreventNotificationIfNoDifference { get; init; }
}