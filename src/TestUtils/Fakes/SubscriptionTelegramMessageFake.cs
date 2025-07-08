using System;
using Domain.Entities.StatData;
using Domain.Entities.StatData.Salary;
using Infrastructure.Database;

namespace TestUtils.Fakes;

public class SubscriptionTelegramMessageFake : SubscriptionTelegramMessage
{
    public SubscriptionTelegramMessageFake(
        StatDataChangeSubscription subscription,
        DateTimeOffset lastMessageDate)
        : base(
            subscription,
            "hello")
    {
        CreatedAt = lastMessageDate;
    }

    public SubscriptionTelegramMessage AsDomain()
    {
        return this;
    }

    public SubscriptionTelegramMessage Please(
        DatabaseContext context)
    {
        var entity = context.Add(AsDomain());
        context.SaveChanges();

        return entity.Entity;
    }
}