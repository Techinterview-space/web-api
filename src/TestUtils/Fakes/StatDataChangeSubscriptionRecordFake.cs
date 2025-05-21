using System;
using System.Linq;
using Domain.Entities.Salaries;
using Domain.Entities.StatData;
using Infrastructure.Database;

namespace TestUtils.Fakes;

public class StatDataChangeSubscriptionRecordFake : StatDataChangeSubscriptionRecord
{
    public StatDataChangeSubscriptionRecordFake(
        StatDataChangeSubscription subscription,
        StatDataChangeSubscriptionRecord previousStatDataChangeSubscriptionRecordOrNull,
        StatDataCacheItemSalaryData data,
        DateTimeOffset? createdAt = null)
        : base(
            subscription,
            previousStatDataChangeSubscriptionRecordOrNull,
            data)
    {
        if (createdAt is not null)
        {
            CreatedAt = createdAt.Value;
        }
    }

    public StatDataChangeSubscriptionRecordFake WithSalaries(
        params UserSalary[] userSalaries)
    {
        Data = new StatDataCacheItemSalaryData(
            userSalaries
                .Select(x => new SalaryBaseData(x))
                .ToList(),
            userSalaries.Length);

        return this;
    }

    public StatDataChangeSubscriptionRecord AsDomain()
    {
        return this;
    }

    public StatDataChangeSubscriptionRecord Please(
        DatabaseContext context)
    {
        var entry = context.Add(AsDomain());
        context.SaveChanges();

        return entry.Entity;
    }
}