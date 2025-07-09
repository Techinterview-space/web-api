using System.ComponentModel.DataAnnotations;
using Domain.Attributes;
using Web.Api.Features.CompanyReviewsSubscriptions.Shared;

namespace Web.Api.Features.CompanyReviewsSubscriptions.CreateSubscription;

public record CreateCompanyReviewsSubscriptionBodyRequest : EditCompanyReviewsSubscriptionBodyRequest
{
    [Required]
    [NotDefaultValue]
    public long TelegramChatId { get; init; }
}