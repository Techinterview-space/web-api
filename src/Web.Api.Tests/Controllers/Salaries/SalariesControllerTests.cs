using System;
using System.Linq;
using System.Threading.Tasks;
using Domain.Entities.Enums;
using Domain.Entities.Salaries;
using Domain.Enums;
using Domain.Exceptions;
using TechInterviewer.Controllers.Salaries;
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
    public async Task Create_ValidData_Ok(
        Role userRole)
    {
        await using var context = new SqliteContext();
        var user = await new FakeUser(userRole).PleaseAsync(context);

        var request = new CreateSalaryRecordRequest
        {
            Value = 100_000,
            Quarter = 1,
            Year = 2023,
            Currency = Currency.KZT,
            Company = CompanyType.Local,
            Grage = DeveloperGrade.Middle
        };

        var salary = await new SalariesController(
            new FakeAuth(user),
            context)
            .Create(
                request,
                default);

        Assert.Equal(request.Value, salary.Value);
        Assert.Equal(request.Quarter, salary.Quarter);
        Assert.Equal(request.Year, salary.Year);
        Assert.Equal(request.Currency, salary.Currency);
        Assert.Equal(request.Company, salary.Company);
        Assert.Equal(request.Grage, salary.Grage);

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
            Grage = DeveloperGrade.Middle
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
        await using var context = new SqliteContext();
        var user = await new FakeUser(Role.Interviewer).PleaseAsync(context);

        var request = new CreateSalaryRecordRequest
        {
            Value = 100_000,
            Quarter = 1,
            Year = year,
            Currency = Currency.KZT,
            Company = CompanyType.Local,
            Grage = DeveloperGrade.Middle
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
        await using var context = new SqliteContext();
        var user = await new FakeUser(Role.Interviewer).PleaseAsync(context);

        var request = new CreateSalaryRecordRequest
        {
            Value = value,
            Quarter = 1,
            Year = 2024,
            Currency = Currency.KZT,
            Company = CompanyType.Local,
            Grage = DeveloperGrade.Middle
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
    public async Task All_ReturnsDataForLastYear_Ok()
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

        var salaries = await new SalariesController(
                new FakeAuth(user),
                context)
            .AllAsync(default);

        Assert.Single(salaries);
        Assert.Contains(salaries, x => Math.Abs(x.Value - salary2.Value) < 0.01);
        Assert.DoesNotContain(salaries, x => Math.Abs(x.Value - salary1.Value) < 0.01);
    }
}