using System;
using System.Collections.Generic;
using System.Linq;
using Domain.Entities.Salaries;
using Domain.Entities.StatData;
using Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace TestUtils.Fakes;

public class StatDataChangeSubscriptionFake : StatDataChangeSubscription
{
    public StatDataChangeSubscriptionFake()
        : base(
            "Random subscription",
            Faker.RandomNumber.Next(111111111, 999999999),
            new List<long>
            {
                (long)UserProfessionEnum.BackendDeveloper,
                (long)UserProfessionEnum.FrontendDeveloper,
            },
            false,
            SubscriptionRegularityType.Weekly,
            false)
    {
    }

    public StatDataChangeSubscriptionFake WithProfession(params UserProfessionEnum[] profession)
    {
        ProfessionIds = new List<long>(profession.Length);
        foreach (var p in profession)
        {
            ProfessionIds.Add((long)p);
        }

        return this;
    }

    public StatDataChangeSubscriptionFake WithLastMessageDate(
        DateTime lastMessageDate)
    {
        StatDataChangeSubscriptionTgMessages ??= new List<StatDataChangeSubscriptionTgMessage>();
        StatDataChangeSubscriptionTgMessages.Add(
            new StatDataChangeSubscriptionTgMessageFake(
                this,
                lastMessageDate)
                .AsDomain());

        return this;
    }

    public StatDataChangeSubscriptionFake WithNoPushesValue(
        bool value)
    {
        PreventNotificationIfNoDifference = value;
        return this;
    }

    public StatDataChangeSubscriptionFake WithRegularity(
        SubscriptionRegularityType regularity)
    {
        Regularity = regularity;
        return this;
    }

    public StatDataChangeSubscription AsDomain()
    {
        return this;
    }

    public StatDataChangeSubscription Please(
        DatabaseContext context)
    {
        var entity = context.Add(AsDomain());
        context.SaveChanges();

        return context.StatDataChangeSubscriptions
            .Include(x => x.StatDataChangeSubscriptionTgMessages)
            .Include(x => x.Records)
            .Include(x => x.AiAnalysisRecords)
            .First(x => x.Id == entity.Entity.Id);
    }
}