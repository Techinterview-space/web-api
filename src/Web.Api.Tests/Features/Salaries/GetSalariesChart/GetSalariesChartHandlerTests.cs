using System;
using System.Linq;
using System.Threading.Tasks;
using Domain.Entities.Salaries;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;
using TestUtils.Auth;
using TestUtils.Db;
using TestUtils.Fakes;
using TestUtils.Mocks;
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
        var user1 = await new UserFake(Role.Interviewer).PleaseAsync(context);
        var user2 = await new UserFake(Role.Interviewer).PleaseAsync(context);
        var user3 = await new UserFake(Role.Interviewer).PleaseAsync(context);

        var salary11 = await context.SaveAsync(new UserSalaryFake(
                user1,
                value: 400_000,
                createdAt: DateTimeOffset.Now.AddYears(-1).AddDays(-190),
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
                createdAt: DateTimeOffset.Now.AddYears(-1).AddDays(-190),
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
                createdAt: DateTimeOffset.Now.AddYears(-1).AddDays(-190),
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
        Assert.Equal(0, salariesResponse.SalariesCount);
        Assert.Equal(802_500, salariesResponse.AverageSalary);
        Assert.Equal(675_000, salariesResponse.MedianSalary);

        Assert.Null(salariesResponse.SalariesByMoneyBarChart);
        Assert.Empty(salariesResponse.Currencies);
    }

    [Fact]
    public async Task Handle_UserHasSalaryForLastQuarter_Ok()
    {
        await using var context = new InMemoryDatabaseContext();
        var user1 = await new UserFake(Role.Interviewer).PleaseAsync(context);
        var user2 = await new UserFake(Role.Interviewer).PleaseAsync(context);
        var user3 = await new UserFake(Role.Interviewer).PleaseAsync(context);

        var salary11 = await context.SaveAsync(new UserSalaryFake(
                user1,
                value: 400_000,
                createdAt: DateTimeOffset.Now.AddYears(-1).AddDays(-190),
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
                createdAt: DateTimeOffset.Now.AddYears(-1).AddDays(-190),
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
                createdAt: DateTimeOffset.Now.AddYears(-1).AddDays(-190),
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
        Assert.Equal(4, salariesResponse.SalariesCount);
        Assert.Equal(802_500, salariesResponse.AverageSalary);
        Assert.Equal(675_000, salariesResponse.MedianSalary);

        Assert.NotNull(salariesResponse.SalariesByMoneyBarChart);
        Assert.Equal(4, salariesResponse.SalariesByMoneyBarChart.Items.Count);
        Assert.Equal(250_000, salariesResponse.SalariesByMoneyBarChart.Step);
        Assert.Equal(3, salariesResponse.SalariesByMoneyBarChart.Items[0]);

        Assert.Equal(3, salariesResponse.Currencies.Count);

        // Test new chart data properties
        Assert.NotNull(salariesResponse.SalariesSkillsChartData);
        Assert.NotNull(salariesResponse.WorkIndustriesChartData);
        Assert.NotNull(salariesResponse.CitiesDoughnutChartData);

        // Test that CitiesDoughnutChartData has empty items (no cities specified in test data)
        Assert.Empty(salariesResponse.CitiesDoughnutChartData.Items);
        Assert.Equal(4, salariesResponse.CitiesDoughnutChartData.NoDataCount);
    }

    [Fact]
    public async Task Handle_UserHasSeveralSalariesForYear_OnlyLastBeingReturned_Ok()
    {
        await using var context = new InMemoryDatabaseContext();
        var user1 = await new UserFake(Role.Interviewer).PleaseAsync(context);

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
        Assert.Equal(4, salariesResponse.SalariesCount);
        Assert.Equal(salary4.Value, salariesResponse.CurrentUserSalary.Value);
        Assert.Equal(3, salariesResponse.Currencies.Count);
    }

    [Fact]
    public async Task Handle_UserHasSalaryWithSkillsAndIndustries_ChartDataPopulated()
    {
        await using var context = new InMemoryDatabaseContext();
        var user1 = await new UserFake(Role.Interviewer).PleaseAsync(context);
        var user2 = await new UserFake(Role.Interviewer).PleaseAsync(context);

        // Create test skills and industries
        var skill1 = new Skill("C#");
        var skill2 = new Skill("JavaScript");
        var industry1 = new WorkIndustry("Finance");
        var industry2 = new WorkIndustry("E-commerce");

        await context.SaveAsync(skill1);
        await context.SaveAsync(skill2);
        await context.SaveAsync(industry1);
        await context.SaveAsync(industry2);

        // Create salaries with skills and industries
        var salary1 = await context.SaveAsync(new UserSalaryFake(
                user1,
                value: 700_000,
                createdAt: DateTimeOffset.Now.AddDays(-1),
                skillOrNull: skill1,
                workIndustryOrNull: industry1,
                kazakhstanCity: KazakhstanCity.Almaty,
                professionOrNull: await context.Professions.FirstAsync(x => x.Id == (long)UserProfessionEnum.Developer))
            .AsDomain());

        var salary2 = await context.SaveAsync(new UserSalaryFake(
                user2,
                value: 600_000,
                createdAt: DateTimeOffset.Now.AddDays(-2),
                skillOrNull: skill2,
                workIndustryOrNull: industry2,
                kazakhstanCity: KazakhstanCity.Astana,
                professionOrNull: await context.Professions.FirstAsync(x => x.Id == (long)UserProfessionEnum.Developer))
            .AsDomain());

        // Salary without skill/industry data
        var salary3 = await context.SaveAsync(new UserSalaryFake(
                user1,
                value: 500_000,
                createdAt: DateTimeOffset.Now.AddDays(-3),
                professionOrNull: await context.Professions.FirstAsync(x => x.Id == (long)UserProfessionEnum.Developer))
            .AsDomain());

        var salariesResponse = await new GetSalariesChartHandler(
                new FakeAuth(user1),
                context,
                new CurrenciesServiceFake())
            .Handle(new GetSalariesChartQuery(), default);

        // Test skills chart data
        Assert.NotNull(salariesResponse.SalariesSkillsChartData);
        Assert.Equal(2, salariesResponse.SalariesSkillsChartData.Items.Count);
        Assert.Equal(1, salariesResponse.SalariesSkillsChartData.NoDataCount); // salary3 has no skill

        var skillItem1 = salariesResponse.SalariesSkillsChartData.Items.FirstOrDefault(x => x.Skill.Title == "C#");
        var skillItem2 = salariesResponse.SalariesSkillsChartData.Items.FirstOrDefault(x => x.Skill.Title == "JavaScript");
        Assert.NotNull(skillItem1);
        Assert.NotNull(skillItem2);
        Assert.Equal(1, skillItem1.Count);
        Assert.Equal(1, skillItem2.Count);

        // Test industries chart data
        Assert.NotNull(salariesResponse.WorkIndustriesChartData);
        Assert.Equal(2, salariesResponse.WorkIndustriesChartData.Items.Count);
        Assert.Equal(1, salariesResponse.WorkIndustriesChartData.NoDataCount); // salary3 has no industry

        var industryItem1 = salariesResponse.WorkIndustriesChartData.Items.FirstOrDefault(x => x.Industry.Title == "Finance");
        var industryItem2 = salariesResponse.WorkIndustriesChartData.Items.FirstOrDefault(x => x.Industry.Title == "E-commerce");
        Assert.NotNull(industryItem1);
        Assert.NotNull(industryItem2);
        Assert.Equal(1, industryItem1.Count);
        Assert.Equal(1, industryItem2.Count);

        // Test cities chart data
        Assert.NotNull(salariesResponse.CitiesDoughnutChartData);
        Assert.Equal(2, salariesResponse.CitiesDoughnutChartData.Items.Count);
        Assert.Equal(1, salariesResponse.CitiesDoughnutChartData.NoDataCount); // salary3 has no city

        var cityItem1 = salariesResponse.CitiesDoughnutChartData.Items.FirstOrDefault(x => x.City == KazakhstanCity.Almaty);
        var cityItem2 = salariesResponse.CitiesDoughnutChartData.Items.FirstOrDefault(x => x.City == KazakhstanCity.Astana);
        Assert.NotNull(cityItem1);
        Assert.NotNull(cityItem2);
        Assert.Equal(1, cityItem1.Count);
        Assert.Equal(1, cityItem2.Count);
    }

    [Fact]
    public async Task Handle_UserHasNoSalaryForLastQuarter_ReturnsFalse()
    {
        await using var context = new InMemoryDatabaseContext();
        var user1 = await new UserFake(Role.Interviewer).PleaseAsync(context);
        var user2 = await new UserFake(Role.Interviewer).PleaseAsync(context);

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
        Assert.Equal(0, salariesResponse.SalariesCount);
        Assert.Empty(salariesResponse.Currencies);

        // Test that chart data properties are null for RequireOwnSalary case
        Assert.Null(salariesResponse.SalariesSkillsChartData);
        Assert.Null(salariesResponse.WorkIndustriesChartData);
        Assert.Null(salariesResponse.CitiesDoughnutChartData);
    }

    [Fact]
    public async Task Handle_AllowReadonly_NoCurrentUser_ReturnsFullChartData()
    {
        await using var context = new InMemoryDatabaseContext();
        var user1 = await new UserFake(Role.Interviewer).PleaseAsync(context);

        await context.SaveAsync(new UserSalaryFake(
                user1,
                value: 700_000,
                createdAt: DateTimeOffset.Now.AddDays(-1),
                professionOrNull: await context.Professions.FirstAsync(x => x.Id == (long)UserProfessionEnum.Developer))
            .AsDomain());

        await context.SaveAsync(new UserSalaryFake(
                user1,
                value: 600_000,
                createdAt: DateTimeOffset.Now.AddDays(-2),
                professionOrNull: await context.Professions.FirstAsync(x => x.Id == (long)UserProfessionEnum.Tester))
            .AsDomain());

        var salariesResponse = await new GetSalariesChartHandler(
                new FakeAuth(null),
                context,
                new CurrenciesServiceFake())
            .Handle(new GetSalariesChartQuery { AllowReadonly = true }, default);

        Assert.False(salariesResponse.ShouldAddOwnSalary);
        Assert.Equal(2, salariesResponse.SalariesCount);
        Assert.Null(salariesResponse.CurrentUserSalary);

        Assert.NotNull(salariesResponse.SalariesSkillsChartData);
        Assert.NotNull(salariesResponse.WorkIndustriesChartData);
        Assert.NotNull(salariesResponse.CitiesDoughnutChartData);
        Assert.NotNull(salariesResponse.GradesMinMaxChartData);
        Assert.NotNull(salariesResponse.ProfessionsDistributionChartData);
        Assert.NotNull(salariesResponse.PeopleByGenderChartData);
        Assert.Equal(3, salariesResponse.Currencies.Count);
    }

    [Fact]
    public async Task Handle_AllowReadonly_UserHasNoSalary_ReturnsFullChartData()
    {
        await using var context = new InMemoryDatabaseContext();
        var user1 = await new UserFake(Role.Interviewer).PleaseAsync(context);
        var user2 = await new UserFake(Role.Interviewer).PleaseAsync(context);

        await context.SaveAsync(new UserSalaryFake(
                user1,
                value: 700_000,
                createdAt: DateTimeOffset.Now.AddDays(-1),
                professionOrNull: await context.Professions.FirstAsync(x => x.Id == (long)UserProfessionEnum.Developer))
            .AsDomain());

        var salariesResponse = await new GetSalariesChartHandler(
                new FakeAuth(user2),
                context,
                new CurrenciesServiceFake())
            .Handle(new GetSalariesChartQuery { AllowReadonly = true }, default);

        Assert.False(salariesResponse.ShouldAddOwnSalary);
        Assert.Equal(1, salariesResponse.SalariesCount);
        Assert.Null(salariesResponse.CurrentUserSalary);

        Assert.NotNull(salariesResponse.SalariesSkillsChartData);
        Assert.NotNull(salariesResponse.WorkIndustriesChartData);
        Assert.NotNull(salariesResponse.CitiesDoughnutChartData);
        Assert.NotNull(salariesResponse.GradesMinMaxChartData);
        Assert.NotNull(salariesResponse.ProfessionsDistributionChartData);
        Assert.NotNull(salariesResponse.PeopleByGenderChartData);
        Assert.Equal(3, salariesResponse.Currencies.Count);
    }

    [Fact]
    public async Task Handle_AllowReadonly_UserHasSalary_ReturnsFullChartDataWithCurrentUserSalary()
    {
        await using var context = new InMemoryDatabaseContext();
        var user1 = await new UserFake(Role.Interviewer).PleaseAsync(context);

        var salary = await context.SaveAsync(new UserSalaryFake(
                user1,
                value: 800_000,
                createdAt: DateTimeOffset.Now.AddDays(-1),
                professionOrNull: await context.Professions.FirstAsync(x => x.Id == (long)UserProfessionEnum.Developer))
            .AsDomain());

        var salariesResponse = await new GetSalariesChartHandler(
                new FakeAuth(user1),
                context,
                new CurrenciesServiceFake())
            .Handle(new GetSalariesChartQuery { AllowReadonly = true }, default);

        Assert.False(salariesResponse.ShouldAddOwnSalary);
        Assert.Equal(1, salariesResponse.SalariesCount);
        Assert.NotNull(salariesResponse.CurrentUserSalary);
        Assert.Equal(salary.Value, salariesResponse.CurrentUserSalary.Value);

        Assert.NotNull(salariesResponse.GradesMinMaxChartData);
        Assert.NotNull(salariesResponse.ProfessionsDistributionChartData);
        Assert.NotNull(salariesResponse.PeopleByGenderChartData);
        Assert.Equal(3, salariesResponse.Currencies.Count);
    }
}