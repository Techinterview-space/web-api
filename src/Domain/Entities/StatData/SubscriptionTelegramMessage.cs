using System;
using Domain.Entities.StatData.CompanyReviews;
using Domain.Entities.StatData.Salary;

namespace Domain.Entities.StatData;

public class SubscriptionTelegramMessage : HasDatesBase, IHasIdBase<Guid>
{
    public Guid Id { get; protected set; }

    public long ChatId { get; protected set; }

    public string Message { get; protected set; }

    public Guid? SalarySubscriptionId { get; protected set; }

    public virtual StatDataChangeSubscription SalarySubscription { get; protected set; }

    public Guid? CompanyReviewsSubscriptionId { get; protected set; }

    public virtual LastWeekCompanyReviewsSubscription CompanyReviewsSubscription { get; protected set; }

    public SubscriptionTelegramMessage(
        StatDataChangeSubscription subscription,
        string message)
    {
        SalarySubscriptionId = subscription.Id;
        ChatId = subscription.TelegramChatId;
        Message = message;
    }

    public SubscriptionTelegramMessage(
        LastWeekCompanyReviewsSubscription subscription,
        string message)
    {
        CompanyReviewsSubscriptionId = subscription.Id;
        ChatId = subscription.TelegramChatId;
        Message = message;
    }

    protected SubscriptionTelegramMessage()
    {
    }
}