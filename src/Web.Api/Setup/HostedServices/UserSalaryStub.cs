using System;
using Domain.Entities.Enums;
using Domain.Entities.Salaries;
using Domain.Enums;
using Domain.ValueObjects.Dates;

namespace Web.Api.Setup.HostedServices;

internal class UserSalaryStub : UserSalary
{
    public UserSalaryStub(
        double value,
        DeveloperGrade? grade,
        CompanyType company,
        Profession profession,
        KazakhstanCity? city)
    {
        Id = Guid.NewGuid();
        UserId = null;

        Value = NonNegativeValue(value);

        var currentQuarter = DateQuarter.Current;
        Quarter = currentQuarter.Quarter;
        Year = currentQuarter.Year;

        Currency = Currency.KZT;
        Grade = grade;
        Company = company;
        ProfessionEnum = profession.IdAsEnum;
        SkillId = null;
        WorkIndustryId = null;
        ProfessionId = profession.Id;

        City = city;
        UseInStats = true;
    }

    public UserSalary AsDomain() => this;
}