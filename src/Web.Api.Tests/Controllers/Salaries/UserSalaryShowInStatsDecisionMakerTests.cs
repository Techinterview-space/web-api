using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Domain.Entities.Enums;
using Domain.Entities.Salaries;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;
using TestUtils.Auth;
using TestUtils.Db;
using TestUtils.Fakes;
using Web.Api.Features.Salaries.Models;
using Xunit;

namespace Web.Api.Tests.Controllers.Salaries;

public class UserSalaryShowInStatsDecisionMakerTests
{
    private static readonly List<double> _salaryValues = new ()
    {
        150_000,
        200_000,
        350_000,
        340_000,
        225_000,
        500_000,
        650_000,
        700_000,
        450_000,
        600_000,
    };

    [Theory]
    [InlineData(85_000, false)]
    [InlineData(140_000, true)]
    [InlineData(200_000, true)]
    [InlineData(700_000, true)]
    [InlineData(3_000_000, false)]
    [InlineData(2_000_000, false)]
    [InlineData(1_000_000, false)]
    public async Task DecideAsync_Cases_MatchAsync(
        double salaryToDecide,
        bool expected)
    {
        await using var context = new InMemoryDatabaseContext();
        var user = await new FakeUser(Role.Interviewer).PleaseAsync(context);

        const DeveloperGrade grade = DeveloperGrade.Middle;
        const CompanyType company = CompanyType.Local;

        var developerProfession = await context.Professions.FirstAsync(x => x.Id == (long)UserProfessionEnum.Developer);
        foreach (var salaryValue in _salaryValues)
        {
            await context.SaveAsync(new UserSalaryFake(
                    user,
                    value: salaryValue,
                    grade: grade,
                    company: company,
                    createdAt: DateTimeOffset.Now.AddDays(-1),
                    professionOrNull: developerProfession)
                .AsDomain());
        }

        var decisionMaker = new UserSalaryShowInStatsDecisionMaker(
            context,
            new FakeCurrentUser(),
            salaryToDecide,
            grade,
            company,
            developerProfession);

        var result = await decisionMaker.DecideAsync(default);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(140_000)]
    [InlineData(200_000)]
    [InlineData(700_000)]
    public async Task DecideAsync_ValidCasesButEmailsIsNotVerified_ReturnFalseAsync(
        double salaryToDecide)
    {
        await using var context = new InMemoryDatabaseContext();
        var user = await new FakeUser(Role.Interviewer).PleaseAsync(context);

        const DeveloperGrade grade = DeveloperGrade.Middle;
        const CompanyType company = CompanyType.Local;

        var developerProfession = await context.Professions.FirstAsync(x => x.Id == (long)UserProfessionEnum.Developer);
        foreach (var salaryValue in _salaryValues)
        {
            await context.SaveAsync(new UserSalaryFake(
                    user,
                    value: salaryValue,
                    grade: grade,
                    company: company,
                    createdAt: DateTimeOffset.Now.AddDays(-1),
                    professionOrNull: developerProfession)
                .AsDomain());
        }

        var decisionMaker = new UserSalaryShowInStatsDecisionMaker(
            context,
            new FakeCurrentUser(isEmailVerified: false),
            salaryToDecide,
            grade,
            company,
            developerProfession);

        var result = await decisionMaker.DecideAsync(default);
        Assert.False(result);
    }
}