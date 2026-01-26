namespace Web.Api.Features.SalarySubscribtions.CreateSubscription;

public record CreateSalarySubscriptionCommand
    : CreateSalarySubscriptionBodyRequest
{
    public CreateSalarySubscriptionCommand(
        CreateSalarySubscriptionBodyRequest request)
    {
        Name = request.Name;
        TelegramChatId = request.TelegramChatId;
        ProfessionIds = request.ProfessionIds;
        PreventNotificationIfNoDifference = request.PreventNotificationIfNoDifference;
        Regularity = request.Regularity;
        UseAiAnalysis = request.UseAiAnalysis;
    }
}