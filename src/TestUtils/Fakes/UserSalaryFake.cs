using System;
using Domain.Entities.Enums;
using Domain.Entities.Salaries;
using Domain.Entities.Users;

namespace TestUtils.Fakes;

public class UserSalaryFake : UserSalary
{
    public UserSalaryFake(
        User user,
        double value = 500_000,
        int quarter = 1,
        int year = 2024,
        Currency currency = Currency.KZT,
        DeveloperGrade? grage = null,
        CompanyType company = CompanyType.Local,
        UserProfession profession = UserProfession.Developer,
        DateTimeOffset? createdAt = null)
        : base(
            user,
            value,
            quarter,
            year,
            currency,
            grage,
            company,
            profession)
    {
        if (createdAt.HasValue)
        {
            CreatedAt = createdAt.Value;
        }
    }

    public UserSalary AsDomain() => this;
}