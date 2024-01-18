using System;
using System.Linq;
using System.Threading.Tasks;
using Domain.Entities.Enums;
using Domain.Entities.Salaries;
using Domain.Enums;
using Domain.Exceptions;
using TechInterviewer.Controllers.Salaries;
using TechInterviewer.Controllers.Salaries.CreateSalaryRecord;
using TechInterviewer.Controllers.Salaries.GetAllSalaries;
using TestUtils.Auth;
using TestUtils.Db;
using TestUtils.Fakes;
using Xunit;

namespace Web.Api.Tests.Controllers.Salaries;

public class SalariesControllerTests
{
    [Theory]
    [InlineData(Role.Admin)]
    [InlineData(Role.Interviewer)]
    public async Task Create_ValidData_HasRecordForThisQuarterAlready_Error(
        Role userRole)
    {
        await using var context = new InMemoryDatabaseContext();
        var user = await new FakeUser(userRole).PleaseAsync(context);

        var salary1 = await context.SaveAsync(new UserSalaryFake(
                user,
                value: 400_000,
                createdAt: DateTimeOffset.Now.AddYears(-1).AddDays(-1))
            .AsDomain());

        var request = new CreateSalaryRecordRequest
        {
            Value = 100_000,
            Quarter = salary1.Quarter,
            Year = salary1.Year,
            Currency = Currency.KZT,
            Company = CompanyType.Local,
            Grade = DeveloperGrade.Middle,
            Profession = UserProfession.Developer,
        };

        Assert.Equal(1, context.Salaries.Count());

        context.ChangeTracker.Clear();
        var result = await new SalariesController(new FakeAuth(user), context)
            .Create(request, default);

        Assert.Equal(1, context.Salaries.Count());
        Assert.False(result.IsSuccess);
        Assert.Equal("You already have a record for this quarter", result.ErrorMessage);
        Assert.Null(result.CreatedSalary);
    }

    [Theory]
    [InlineData(Role.Admin)]
    [InlineData(Role.Interviewer)]
    public async Task Create_HasRecordForThisQuarterAlready_FailedResult(
        Role userRole)
    {
        await using var context = new InMemoryDatabaseContext();
        var user = await new FakeUser(userRole).PleaseAsync(context);

        var request = new CreateSalaryRecordRequest
        {
            Value = 100_000,
            Quarter = 1,
            Year = 2023,
            Currency = Currency.KZT,
            Company = CompanyType.Local,
            Grade = DeveloperGrade.Middle,
            Profession = UserProfession.ProductOwner,
        };

        var result = await new SalariesController(
                new FakeAuth(user),
                context)
            .Create(
                request,
                default);

        Assert.True(result.IsSuccess);
        Assert.Null(result.ErrorMessage);
        var salary = result.CreatedSalary;

        Assert.Equal(request.Value, salary.Value);
        Assert.Equal(request.Quarter, salary.Quarter);
        Assert.Equal(request.Year, salary.Year);
        Assert.Equal(request.Currency, salary.Currency);
        Assert.Equal(request.Company, salary.Company);
        Assert.Equal(request.Grade, salary.Grade);

        Assert.Equal(1, context.Salaries.Count());
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(5)]
    public async Task Create_QuarterInvalidValue_Error(
        int quarter)
    {
        await using var context = new SqliteContext();
        var user = await new FakeUser(Role.Interviewer).PleaseAsync(context);

        var request = new CreateSalaryRecordRequest
        {
            Value = 100_000,
            Quarter = quarter,
            Year = 2023,
            Currency = Currency.KZT,
            Company = CompanyType.Local,
            Grade = DeveloperGrade.Middle
        };

        await Assert.ThrowsAsync<BadRequestException>(() =>
            new SalariesController(
                    new FakeAuth(user),
                    context)
                .Create(
                    request,
                    default));

        Assert.Equal(0, context.Salaries.Count());
    }

    [Theory]
    [InlineData(1999)]
    [InlineData(3000)]
    [InlineData(-1)]
    public async Task Create_YearInvalidValue_Error(
        int year)
    {
        await using var context = new InMemoryDatabaseContext();
        var user = await new FakeUser(Role.Interviewer).PleaseAsync(context);

        var request = new CreateSalaryRecordRequest
        {
            Value = 100_000,
            Quarter = 1,
            Year = year,
            Currency = Currency.KZT,
            Company = CompanyType.Local,
            Grade = DeveloperGrade.Middle
        };

        await Assert.ThrowsAsync<BadRequestException>(() =>
            new SalariesController(
                    new FakeAuth(user),
                    context)
                .Create(
                    request,
                    default));

        Assert.Equal(0, context.Salaries.Count());
    }

    [Theory]
    [InlineData(-100000)]
    [InlineData(0)]
    public async Task Create_ValueInvalidValue_Error(
        double value)
    {
        await using var context = new InMemoryDatabaseContext();
        var user = await new FakeUser(Role.Interviewer).PleaseAsync(context);

        var request = new CreateSalaryRecordRequest
        {
            Value = value,
            Quarter = 1,
            Year = 2024,
            Currency = Currency.KZT,
            Company = CompanyType.Local,
            Grade = DeveloperGrade.Middle
        };

        await Assert.ThrowsAsync<BadRequestException>(() =>
            new SalariesController(
                    new FakeAuth(user),
                    context)
                .Create(
                    request,
                    default));

        Assert.Equal(0, context.Salaries.Count());
    }

    [Fact]
    public async Task All_ReturnsAllData_Ok()
    {
        await using var context = new InMemoryDatabaseContext();
        var user = await new FakeUser(Role.Interviewer).PleaseAsync(context);

        var salary1 = await context.SaveAsync(new UserSalaryFake(
            user,
            value: 400_000,
            createdAt: DateTimeOffset.Now.AddYears(-1).AddDays(-1))
            .AsDomain());

        var salary2 = await context.SaveAsync(new UserSalaryFake(
            user,
            value: 600_000,
            createdAt: DateTimeOffset.Now.AddDays(-1))
            .AsDomain());

        var createdSalaries = context.Salaries.ToList();
        Assert.Equal(2, createdSalaries.Count);

        var salaries = await new SalariesController(new FakeAuth(user), context)
            .AllAsync(
                new GetAllSalariesRequest(),
                default);

        Assert.Equal(2, salaries.TotalItems);
        Assert.Equal(2, salaries.Results.Count);
    }

    [Fact]
    public async Task Chart_UserHasSalaryForLastQuarter_Ok()
    {
        await using var context = new InMemoryDatabaseContext();
        var user1 = await new FakeUser(Role.Interviewer).PleaseAsync(context);
        var user2 = await new FakeUser(Role.Interviewer).PleaseAsync(context);
        var user3 = await new FakeUser(Role.Interviewer).PleaseAsync(context);

        var salary11 = await context.SaveAsync(new UserSalaryFake(
                user1,
                value: 400_000,
                createdAt: DateTimeOffset.Now.AddYears(-1).AddDays(-1))
            .AsDomain());

        var salary12 = await context.SaveAsync(new UserSalaryFake(
                user1,
                value: 700_000,
                createdAt: DateTimeOffset.Now.AddDays(-1))
            .AsDomain());

        var salary21 = await context.SaveAsync(new UserSalaryFake(
                user2,
                value: 400_000,
                createdAt: DateTimeOffset.Now.AddYears(-1).AddDays(-1))
            .AsDomain());

        var salary22 = await context.SaveAsync(new UserSalaryFake(
                user2,
                value: 600_000,
                profession: UserProfession.BusinessAnalyst,
                createdAt: DateTimeOffset.Now.AddDays(-4))
            .AsDomain());

        var salary31 = await context.SaveAsync(new UserSalaryFake(
                user3,
                value: 400_000,
                createdAt: DateTimeOffset.Now.AddYears(-1).AddDays(-1))
            .AsDomain());

        var salary32 = await context.SaveAsync(new UserSalaryFake(
                user3,
                value: 650_000,
                profession: UserProfession.Tester,
                createdAt: DateTimeOffset.Now.AddDays(-2))
            .AsDomain());

        var salary33 = await context.SaveAsync(new UserSalaryFake(
                user3,
                value: 1_260_000,
                profession: UserProfession.Tester,
                createdAt: DateTimeOffset.Now.AddDays(-2))
            .AsDomain());

        var createdSalaries = context.Salaries.ToList();
        Assert.Equal(7, createdSalaries.Count);

        var salariesResponse = await new SalariesController(
                new FakeAuth(user1),
                context)
            .ChartAsync(default);

        Assert.False(salariesResponse.ShouldAddOwnSalary);
        Assert.Equal(4, salariesResponse.Salaries.Count);
        Assert.Equal(802_500, salariesResponse.AverageSalary);
        Assert.Equal(675_000, salariesResponse.MedianSalary);

        Assert.NotNull(salariesResponse.SalariesByMoneyBarChart);
        Assert.Equal(4, salariesResponse.SalariesByMoneyBarChart.Items.Count);
        Assert.Equal(250_000, salariesResponse.SalariesByMoneyBarChart.Step);
        Assert.Equal(3, salariesResponse.SalariesByMoneyBarChart.Items[0].Count);

        Assert.Equal(3, salariesResponse.SalariesByMoneyBarChart.ItemsByProfession.Count);
        Assert.Equal(
            UserProfession.Developer,
            salariesResponse.SalariesByMoneyBarChart.ItemsByProfession[0].Profession);
        Assert.Equal(
            UserProfession.BusinessAnalyst,
            salariesResponse.SalariesByMoneyBarChart.ItemsByProfession[1].Profession);
        Assert.Equal(
            UserProfession.Tester,
            salariesResponse.SalariesByMoneyBarChart.ItemsByProfession[2].Profession);
    }

    [Fact]
    public async Task Chart_UserHasSeveralSalariesForYear_OnlyLastBeingReturned_Ok()
    {
        await using var context = new InMemoryDatabaseContext();
        var user1 = await new FakeUser(Role.Interviewer).PleaseAsync(context);

        var salary1 = await context.SaveAsync(new UserSalaryFake(
                user1,
                value: 400_000,
                quarter: 2,
                year: DateTimeOffset.Now.Year - 1,
                createdAt: DateTimeOffset.Now.AddDays(-1))
            .AsDomain());

        var salary2 = await context.SaveAsync(new UserSalaryFake(
                user1,
                value: 300_000,
                quarter: 1,
                year: DateTimeOffset.Now.Year - 1,
                createdAt: DateTimeOffset.Now.AddDays(-1))
            .AsDomain());

        var salary3 = await context.SaveAsync(new UserSalaryFake(
                user1,
                value: 450_000,
                quarter: 4,
                year: DateTimeOffset.Now.Year - 1,
                createdAt: DateTimeOffset.Now.AddDays(-1))
            .AsDomain());

        var salary4 = await context.SaveAsync(new UserSalaryFake(
                user1,
                value: 500_000,
                quarter: 1,
                year: DateTimeOffset.Now.Year,
                createdAt: DateTimeOffset.Now.AddDays(-1))
            .AsDomain());

        var createdSalaries = context.Salaries.ToList();
        Assert.Equal(4, createdSalaries.Count);

        var salariesResponse = await new SalariesController(
                new FakeAuth(user1),
                context)
            .ChartAsync(default);

        Assert.False(salariesResponse.ShouldAddOwnSalary);
        Assert.Equal(4, salariesResponse.Salaries.Count);
        Assert.Equal(salary4.Value, salariesResponse.CurrentUserSalary.Value);
    }

    [Fact]
    public async Task Chart_UserHasNoSalaryForLastQuarter_ReturnsFalse()
    {
        await using var context = new InMemoryDatabaseContext();
        var user1 = await new FakeUser(Role.Interviewer).PleaseAsync(context);
        var user2 = await new FakeUser(Role.Interviewer).PleaseAsync(context);

        var salary11 = await context.SaveAsync(new UserSalaryFake(
                user1,
                value: 400_000,
                createdAt: DateTimeOffset.Now.AddYears(-1).AddDays(-1))
            .AsDomain());

        var salary12 = await context.SaveAsync(new UserSalaryFake(
                user1,
                value: 600_000,
                createdAt: DateTimeOffset.Now.AddDays(-1))
            .AsDomain());

        var createdSalaries = context.Salaries.ToList();
        Assert.Equal(2, createdSalaries.Count);

        var salariesResponse = await new SalariesController(
                new FakeAuth(user2),
                context)
            .ChartAsync(default);

        Assert.True(salariesResponse.ShouldAddOwnSalary);
        Assert.Empty(salariesResponse.Salaries);
    }

    [Fact]
    public async Task Delete_SalaryDoesExist_Ok()
    {
        await using var context = new InMemoryDatabaseContext();
        var user1 = await new FakeUser(Role.Admin).PleaseAsync(context);

        var salary11 = await context.SaveAsync(new UserSalaryFake(
                user1,
                value: 400_000,
                createdAt: DateTimeOffset.Now.AddYears(-1).AddDays(-1))
            .AsDomain());

        var salary12 = await context.SaveAsync(new UserSalaryFake(
                user1,
                value: 600_000,
                createdAt: DateTimeOffset.Now.AddDays(-1))
            .AsDomain());

        var allSalaries = context.Salaries.ToList();
        Assert.Equal(2, allSalaries.Count);

        await new SalariesController(new FakeAuth(user1), context)
            .Delete(salary12.Id, default);

        allSalaries = context.Salaries.ToList();
        Assert.Single(allSalaries);
        Assert.Equal(salary11.Id, allSalaries[0].Id);
    }
}