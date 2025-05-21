using System;
using System.Threading.Tasks;
using Domain.Entities.Enums;
using Domain.Entities.Salaries;
using Domain.Entities.Users;
using Domain.Enums;
using Infrastructure.Database;

namespace TestUtils.Fakes;

public class UserSalaryFake : UserSalary
{
    public UserSalaryFake(
        User user,
        double value = 500_000,
        int quarter = 1,
        int year = 2025,
        Currency currency = Currency.KZT,
        DeveloperGrade? grade = null,
        CompanyType company = CompanyType.Local,
        DateTimeOffset? createdAt = null,
        Skill skillOrNull = null,
        WorkIndustry workIndustryOrNull = null,
        Profession professionOrNull = null,
        KazakhstanCity? kazakhstanCity = null,
        bool useInStats = true,
        SalarySourceType? sourceType = null)
        : base(
            user,
            value,
            quarter,
            year,
            currency,
            grade,
            company,
            skillOrNull,
            workIndustryOrNull,
            professionOrNull,
            kazakhstanCity,
            useInStats,
            sourceType)
    {
        if (createdAt.HasValue)
        {
            CreatedAt = createdAt.Value;
        }
    }

    public UserSalaryFake WithProfession(
        UserProfessionEnum profession)
    {
        ProfessionId = (long)profession;
        return this;
    }

    public UserSalary AsDomain() => this;

    public async Task<UserSalary> PleaseAsync(DatabaseContext context)
    {
        var entry = await context.Salaries.AddAsync(AsDomain());
        await context.TrySaveChangesAsync();
        return await context.Salaries
            .ByIdOrFailAsync(entry.Entity.Id);
    }
}