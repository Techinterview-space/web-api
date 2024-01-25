﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Domain.Entities.Enums;
using Domain.Entities.Salaries;
using Domain.Enums;
using TechInterviewer.Controllers.Salaries;
using TestUtils.Db;
using TestUtils.Fakes;
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
        const UserProfession profession = UserProfession.Developer;

        foreach (var salaryValue in _salaryValues)
        {
            await context.SaveAsync(new UserSalaryFake(
                    user,
                    value: salaryValue,
                    grade: grade,
                    company: company,
                    createdAt: DateTimeOffset.Now.AddDays(-1))
                .AsDomain());
        }

        var decisionMaker = new UserSalaryShowInStatsDecisionMaker(
            context,
            salaryToDecide,
            grade,
            company,
            profession);

        var result = await decisionMaker.DecideAsync(default);
        Assert.Equal(expected, result);
    }
}