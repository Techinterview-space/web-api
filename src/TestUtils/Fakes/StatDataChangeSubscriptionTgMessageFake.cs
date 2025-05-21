using System;
using Domain.Entities.StatData;
using Infrastructure.Database;

namespace TestUtils.Fakes;

public class StatDataChangeSubscriptionTgMessageFake : StatDataChangeSubscriptionTgMessage
{
    public StatDataChangeSubscriptionTgMessageFake(
        StatDataChangeSubscription subscription,
        DateTimeOffset lastMessageDate)
        : base(
            subscription,
            "hello")
    {
        CreatedAt = lastMessageDate;
    }

    public StatDataChangeSubscriptionTgMessage AsDomain()
    {
        return this;
    }

    public StatDataChangeSubscriptionTgMessage Please(
        DatabaseContext context)
    {
        var entity = context.Add(AsDomain());
        context.SaveChanges();

        return entity.Entity;
    }
}