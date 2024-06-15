using System;
using System.Linq;
using System.Threading.Tasks;
using Domain.Entities.Enums;
using Domain.Entities.Salaries;
using Domain.Enums;
using Domain.Validation.Exceptions;
using TestUtils.Auth;
using TestUtils.Db;
using TestUtils.Fakes;
using Web.Api.Features.Salaries.AddSalary;
using Xunit;

namespace Web.Api.Tests.Features.Salaries.AddSalary;

public class AddSalaryHandlerTests
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

        var request = new AddSalaryCommand
        {
            Value = 100_000,
            Quarter = salary1.Quarter,
            Year = salary1.Year,
            Currency = Currency.KZT,
            Company = CompanyType.Local,
            Grade = DeveloperGrade.Middle,
            ProfessionId = (long)UserProfessionEnum.Developer,
        };

        Assert.Equal(1, context.Salaries.Count());

        context.ChangeTracker.Clear();
        var result = await new AddSalaryHandler(
                new FakeAuth(user),
                context)
            .Handle(request, default);

        Assert.Equal(1, context.Salaries.Count());
        Assert.False(result.IsSuccess);
        Assert.Equal("You already have a record for this quarter", result.ErrorMessage);
        Assert.Null(result.CreatedSalary);
    }

    [Theory]
    [InlineData(Role.Admin)]
    [InlineData(Role.Interviewer)]
    public async Task Create_HasNoRecordForThisQuarterAlready_Ok(
        Role userRole)
    {
        await using var context = new InMemoryDatabaseContext();
        var user = await new FakeUser(userRole).PleaseAsync(context);

        var request = new AddSalaryCommand
        {
            Value = 100_000,
            Quarter = 1,
            Year = 2023,
            Currency = Currency.KZT,
            Company = CompanyType.Local,
            Grade = DeveloperGrade.Middle,
            ProfessionId = (long)UserProfessionEnum.ProductOwner,
        };

        var result = await new AddSalaryHandler(
                new FakeAuth(user),
                context)
            .Handle(
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

        var request = new AddSalaryCommand
        {
            Value = 100_000,
            Quarter = quarter,
            Year = 2023,
            Currency = Currency.KZT,
            Company = CompanyType.Local,
            Grade = DeveloperGrade.Middle
        };

        await Assert.ThrowsAsync<BadRequestException>(() =>
            new AddSalaryHandler(
                    new FakeAuth(user),
                    context)
                .Handle(
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

        var request = new AddSalaryCommand
        {
            Value = 100_000,
            Quarter = 1,
            Year = year,
            Currency = Currency.KZT,
            Company = CompanyType.Local,
            Grade = DeveloperGrade.Middle
        };

        await Assert.ThrowsAsync<BadRequestException>(() =>
            new AddSalaryHandler(
                    new FakeAuth(user),
                    context)
                .Handle(
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

        var request = new AddSalaryCommand
        {
            Value = value,
            Quarter = 1,
            Year = 2024,
            Currency = Currency.KZT,
            Company = CompanyType.Local,
            Grade = DeveloperGrade.Middle
        };

        await Assert.ThrowsAsync<BadRequestException>(() =>
            new AddSalaryHandler(
                    new FakeAuth(user),
                    context)
                .Handle(
                    request,
                    default));

        Assert.Equal(0, context.Salaries.Count());
    }
}