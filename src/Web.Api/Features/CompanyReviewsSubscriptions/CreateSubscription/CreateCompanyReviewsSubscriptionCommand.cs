namespace Web.Api.Features.CompanyReviewsSubscriptions.CreateSubscription;

public record CreateCompanyReviewsSubscriptionCommand
    : CreateCompanyReviewsSubscriptionBodyRequest
{
    public CreateCompanyReviewsSubscriptionCommand(
        CreateCompanyReviewsSubscriptionBodyRequest request)
    {
        Name = request.Name;
        TelegramChatId = request.TelegramChatId;
        Regularity = request.Regularity;
        UseAiAnalysis = request.UseAiAnalysis;
    }
}