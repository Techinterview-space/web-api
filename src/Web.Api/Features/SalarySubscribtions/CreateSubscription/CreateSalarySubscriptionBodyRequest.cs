using System.ComponentModel.DataAnnotations;
using Domain.Attributes;
using Web.Api.Features.SalarySubscribtions.Shared;

namespace Web.Api.Features.SalarySubscribtions.CreateSubscription;

public record CreateSalarySubscriptionBodyRequest : EditSalarySubscriptionBodyRequest
{
    [Required]
    [NotDefaultValue]
    public long TelegramChatId { get; init; }
}