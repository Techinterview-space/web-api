using System;
using System.Threading.Tasks;
using Domain.Database;
using Domain.Entities.Enums;
using Domain.Entities.Salaries;
using Domain.Entities.Users;
using Microsoft.EntityFrameworkCore;

namespace TestUtils.Fakes;

public class UserSalaryFake : UserSalary
{
    public UserSalaryFake(
        User user,
        double value = 500_000,
        int quarter = 1,
        int year = 2024,
        Currency currency = Currency.KZT,
        DeveloperGrade? grade = null,
        CompanyType company = CompanyType.Local,
        UserProfession profession = UserProfession.Developer,
        DateTimeOffset? createdAt = null,
        Skill skill = null,
        bool useInStats = true)
        : base(
            user,
            value,
            quarter,
            year,
            currency,
            grade,
            company,
            profession,
            skill?.Id,
            useInStats)
    {
        if (createdAt.HasValue)
        {
            CreatedAt = createdAt.Value;
        }
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