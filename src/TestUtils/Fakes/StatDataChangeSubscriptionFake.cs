using System.Collections.Generic;
using Domain.Entities.Salaries;
using Domain.Entities.StatData;

namespace TestUtils.Fakes;

public class StatDataChangeSubscriptionFake : StatDataChangeSubscription
{
    public StatDataChangeSubscriptionFake()
        : base(
            "Random subscription",
            Faker.RandomNumber.Next(11111111111, 999999999),
            new List<long>
            {
                (long)UserProfessionEnum.BackendDeveloper,
                (long)UserProfessionEnum.FrontendDeveloper,
            },
            false)
    {
    }
}