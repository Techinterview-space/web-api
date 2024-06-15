using System;
using System.Linq;
using System.Threading.Tasks;
using Domain.Entities.Salaries;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;
using TestUtils.Auth;
using TestUtils.Db;
using TestUtils.Fakes;
using Web.Api.Features.Salaries.GetSalariesChart;
using Web.Api.Tests.Mocks;
using Xunit;

namespace Web.Api.Tests.Features.Salaries.GetSalariesChart;

public class GetSalariesChartHandlerTests
{
    [Fact]
    public async Task Handle_NoCurrentUser_Ok()
    {
        await using var context = new InMemoryDatabaseContext();
        var user1 = await new FakeUser(Role.Interviewer).PleaseAsync(context);
        var user2 = await new FakeUser(Role.Interviewer).PleaseAsync(context);
        var user3 = await new FakeUser(Role.Interviewer).PleaseAsync(context);

        var salary11 = await context.SaveAsync(new UserSalaryFake(
                user1,
                value: 400_000,
                createdAt: DateTimeOffset.Now.AddYears(-1).AddDays(-1),
                professionOrNull: await context.Professions.FirstAsync(x => x.Id == (long)UserProfessionEnum.Developer))
            .AsDomain());

        var salary12 = await context.SaveAsync(new UserSalaryFake(
                user1,
                value: 700_000,
                createdAt: DateTimeOffset.Now.AddDays(-1),
                professionOrNull: await context.Professions.FirstAsync(x => x.Id == (long)UserProfessionEnum.Developer))
            .AsDomain());

        var salary21 = await context.SaveAsync(new UserSalaryFake(
                user2,
                value: 400_000,
                createdAt: DateTimeOffset.Now.AddYears(-1).AddDays(-1),
                professionOrNull: await context.Professions.FirstAsync(x => x.Id == (long)UserProfessionEnum.Developer))
            .AsDomain());

        var salary22 = await context.SaveAsync(new UserSalaryFake(
                user2,
                value: 600_000,
                createdAt: DateTimeOffset.Now.AddDays(-4),
                professionOrNull: await context.Professions.FirstAsync(x => x.Id == (long)UserProfessionEnum.Developer))
            .AsDomain());

        var salary31 = await context.SaveAsync(new UserSalaryFake(
                user3,
                value: 400_000,
                createdAt: DateTimeOffset.Now.AddYears(-1).AddDays(-1),
                professionOrNull: await context.Professions.FirstAsync(x => x.Id == (long)UserProfessionEnum.Developer))
            .AsDomain());

        var salary32 = await context.SaveAsync(new UserSalaryFake(
                user3,
                value: 650_000,
                createdAt: DateTimeOffset.Now.AddDays(-2),
                professionOrNull: await context.Professions.FirstAsync(x => x.Id == (long)UserProfessionEnum.Tester))
            .AsDomain());

        var salary33 = await context.SaveAsync(new UserSalaryFake(
                user3,
                value: 1_260_000,
                createdAt: DateTimeOffset.Now.AddDays(-2),
                professionOrNull: await context.Professions.FirstAsync(x => x.Id == (long)UserProfessionEnum.Tester))
            .AsDomain());

        var createdSalaries = context.Salaries.ToList();
        Assert.Equal(7, createdSalaries.Count);

        var salariesResponse = await new GetSalariesChartHandler(
                new FakeAuth(null),
                context,
                new CurrenciesServiceFake())
            .Handle(new GetSalariesChartQuery(), default);

        Assert.True(salariesResponse.ShouldAddOwnSalary);
        Assert.Equal(4, salariesResponse.TotalCountInStats);
        Assert.Empty(salariesResponse.Salaries);
        Assert.Equal(802_500, salariesResponse.AverageSalary);
        Assert.Equal(675_000, salariesResponse.MedianSalary);

        Assert.Null(salariesResponse.SalariesByMoneyBarChart);
        Assert.Empty(salariesResponse.Currencies);
    }

    [Fact]
    public async Task Handle_UserHasSalaryForLastQuarter_Ok()
    {
        await using var context = new InMemoryDatabaseContext();
        var user1 = await new FakeUser(Role.Interviewer).PleaseAsync(context);
        var user2 = await new FakeUser(Role.Interviewer).PleaseAsync(context);
        var user3 = await new FakeUser(Role.Interviewer).PleaseAsync(context);

        var salary11 = await context.SaveAsync(new UserSalaryFake(
                user1,
                value: 400_000,
                createdAt: DateTimeOffset.Now.AddYears(-1).AddDays(-1),
                professionOrNull: await context.Professions.FirstAsync(x => x.Id == (long)UserProfessionEnum.Developer))
            .AsDomain());

        var salary12 = await context.SaveAsync(new UserSalaryFake(
                user1,
                value: 700_000,
                createdAt: DateTimeOffset.Now.AddDays(-1),
                professionOrNull: await context.Professions.FirstAsync(x => x.Id == (long)UserProfessionEnum.Developer))
            .AsDomain());

        var salary21 = await context.SaveAsync(new UserSalaryFake(
                user2,
                value: 400_000,
                createdAt: DateTimeOffset.Now.AddYears(-1).AddDays(-1),
                professionOrNull: await context.Professions.FirstAsync(x => x.Id == (long)UserProfessionEnum.Developer))
            .AsDomain());

        var salary22 = await context.SaveAsync(new UserSalaryFake(
                user2,
                value: 600_000,
                createdAt: DateTimeOffset.Now.AddDays(-4),
                professionOrNull: await context.Professions.FirstAsync(x => x.Id == (long)UserProfessionEnum.BusinessAnalyst))
            .AsDomain());

        var salary31 = await context.SaveAsync(new UserSalaryFake(
                user3,
                value: 400_000,
                createdAt: DateTimeOffset.Now.AddYears(-1).AddDays(-1),
                professionOrNull: await context.Professions.FirstAsync(x => x.Id == (long)UserProfessionEnum.Developer))
            .AsDomain());

        var salary32 = await context.SaveAsync(new UserSalaryFake(
                user3,
                value: 650_000,
                createdAt: DateTimeOffset.Now.AddDays(-2),
                professionOrNull: await context.Professions.FirstAsync(x => x.Id == (long)UserProfessionEnum.Tester))
            .AsDomain());

        var salary33 = await context.SaveAsync(new UserSalaryFake(
                user3,
                value: 1_260_000,
                createdAt: DateTimeOffset.Now.AddDays(-2),
                professionOrNull: await context.Professions.FirstAsync(x => x.Id == (long)UserProfessionEnum.Tester))
            .AsDomain());

        var createdSalaries = context.Salaries.ToList();
        Assert.Equal(7, createdSalaries.Count);

        var salariesResponse = await new GetSalariesChartHandler(
                new FakeAuth(user1),
                context,
                new CurrenciesServiceFake())
            .Handle(new GetSalariesChartQuery(), default);

        Assert.False(salariesResponse.ShouldAddOwnSalary);
        Assert.Equal(4, salariesResponse.Salaries.Count);
        Assert.Equal(802_500, salariesResponse.AverageSalary);
        Assert.Equal(675_000, salariesResponse.MedianSalary);

        Assert.NotNull(salariesResponse.SalariesByMoneyBarChart);
        Assert.Equal(4, salariesResponse.SalariesByMoneyBarChart.Items.Count);
        Assert.Equal(250_000, salariesResponse.SalariesByMoneyBarChart.Step);
        Assert.Equal(3, salariesResponse.SalariesByMoneyBarChart.Items[0]);

        Assert.Equal(3, salariesResponse.SalariesByMoneyBarChart.ItemsByProfession.Count);
        Assert.Equal(
            (long)UserProfessionEnum.BusinessAnalyst,
            salariesResponse.SalariesByMoneyBarChart.ItemsByProfession[0].ProfessionId);
        Assert.Equal(
            (long)UserProfessionEnum.Tester,
            salariesResponse.SalariesByMoneyBarChart.ItemsByProfession[1].ProfessionId);
        Assert.Equal(
            (long)UserProfessionEnum.Developer,
            salariesResponse.SalariesByMoneyBarChart.ItemsByProfession[2].ProfessionId);

        Assert.Equal(3, salariesResponse.Currencies.Count);
    }

    [Fact]
    public async Task Handle_UserHasSeveralSalariesForYear_OnlyLastBeingReturned_Ok()
    {
        await using var context = new InMemoryDatabaseContext();
        var user1 = await new FakeUser(Role.Interviewer).PleaseAsync(context);

        var salary1 = await context.SaveAsync(new UserSalaryFake(
                user1,
                value: 400_000,
                quarter: 2,
                year: DateTimeOffset.Now.Year - 1,
                createdAt: DateTimeOffset.Now.AddDays(-1),
                professionOrNull: await context.Professions.FirstAsync(x => x.Id == (long)UserProfessionEnum.Developer))
            .AsDomain());

        var salary2 = await context.SaveAsync(new UserSalaryFake(
                user1,
                value: 300_000,
                quarter: 1,
                year: DateTimeOffset.Now.Year - 1,
                createdAt: DateTimeOffset.Now.AddDays(-1),
                professionOrNull: await context.Professions.FirstAsync(x => x.Id == (long)UserProfessionEnum.Developer))
            .AsDomain());

        var salary3 = await context.SaveAsync(new UserSalaryFake(
                user1,
                value: 450_000,
                quarter: 4,
                year: DateTimeOffset.Now.Year - 1,
                createdAt: DateTimeOffset.Now.AddDays(-1),
                professionOrNull: await context.Professions.FirstAsync(x => x.Id == (long)UserProfessionEnum.Developer))
            .AsDomain());

        var salary4 = await context.SaveAsync(new UserSalaryFake(
                user1,
                value: 500_000,
                quarter: 1,
                year: DateTimeOffset.Now.Year,
                createdAt: DateTimeOffset.Now.AddDays(-1),
                professionOrNull: await context.Professions.FirstAsync(x => x.Id == (long)UserProfessionEnum.Developer))
            .AsDomain());

        var createdSalaries = context.Salaries.ToList();
        Assert.Equal(4, createdSalaries.Count);

        var salariesResponse = await new GetSalariesChartHandler(
                new FakeAuth(user1),
                context,
                new CurrenciesServiceFake())
            .Handle(new GetSalariesChartQuery(), default);

        Assert.False(salariesResponse.ShouldAddOwnSalary);
        Assert.Equal(4, salariesResponse.Salaries.Count);
        Assert.Equal(salary4.Value, salariesResponse.CurrentUserSalary.Value);
        Assert.Equal(3, salariesResponse.Currencies.Count);
    }

    [Fact]
    public async Task Handle_UserHasNoSalaryForLastQuarter_ReturnsFalse()
    {
        await using var context = new InMemoryDatabaseContext();
        var user1 = await new FakeUser(Role.Interviewer).PleaseAsync(context);
        var user2 = await new FakeUser(Role.Interviewer).PleaseAsync(context);

        var salary11 = await context.SaveAsync(new UserSalaryFake(
                user1,
                value: 400_000,
                createdAt: DateTimeOffset.Now.AddYears(-1).AddDays(-1),
                professionOrNull: await context.Professions.FirstAsync(x => x.Id == (long)UserProfessionEnum.Developer))
            .AsDomain());

        var salary12 = await context.SaveAsync(new UserSalaryFake(
                user1,
                value: 600_000,
                createdAt: DateTimeOffset.Now.AddDays(-1),
                professionOrNull: await context.Professions.FirstAsync(x => x.Id == (long)UserProfessionEnum.Developer))
            .AsDomain());

        var createdSalaries = context.Salaries.ToList();
        Assert.Equal(2, createdSalaries.Count);

        var salariesResponse = await new GetSalariesChartHandler(
                new FakeAuth(user2),
                context,
                new CurrenciesServiceFake())
            .Handle(new GetSalariesChartQuery(), default);

        Assert.True(salariesResponse.ShouldAddOwnSalary);
        Assert.Empty(salariesResponse.Salaries);
        Assert.Empty(salariesResponse.Currencies);
    }
}