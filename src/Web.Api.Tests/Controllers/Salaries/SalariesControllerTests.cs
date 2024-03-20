using System;
using System.Linq;
using System.Threading.Tasks;
using Domain.Entities.Enums;
using Domain.Entities.Salaries;
using Domain.Enums;
using Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Moq;
using TechInterviewer.Controllers.Salaries;
using TechInterviewer.Controllers.Salaries.CreateSalaryRecord;
using TechInterviewer.Features.Salaries;
using TechInterviewer.Features.Salaries.Admin;
using TechInterviewer.Features.Salaries.GetSalariesChart.Charts;
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

        var request = new CreateOrEditSalaryRecordRequest
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
        var result = await new SalariesController(
                new FakeAuth(user),
                context,
                new GlobalFake(),
                new Mock<IMediator>().Object)
            .Create(request, default);

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

        var request = new CreateOrEditSalaryRecordRequest
        {
            Value = 100_000,
            Quarter = 1,
            Year = 2023,
            Currency = Currency.KZT,
            Company = CompanyType.Local,
            Grade = DeveloperGrade.Middle,
            ProfessionId = (long)UserProfessionEnum.ProductOwner,
        };

        var result = await new SalariesController(
                new FakeAuth(user),
                context,
                new GlobalFake(),
                new Mock<IMediator>().Object)
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

        var request = new CreateOrEditSalaryRecordRequest
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
                    context,
                    new GlobalFake(),
                    new Mock<IMediator>().Object)
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

        var request = new CreateOrEditSalaryRecordRequest
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
                    context,
                    new GlobalFake(),
                    new Mock<IMediator>().Object)
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

        var request = new CreateOrEditSalaryRecordRequest
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
                    context,
                    new GlobalFake(),
                    new Mock<IMediator>().Object)
                .Create(
                    request,
                    default));

        Assert.Equal(0, context.Salaries.Count());
    }

    [Fact]
    public async Task Update_HasNoRecordForNewQuarterAlready_Ok()
    {
        await using var context = new InMemoryDatabaseContext();
        var user = await new FakeUser(Role.Interviewer).PleaseAsync(context);

        var profession = await context.Professions.FirstAsync(x => x.Id == (long)UserProfessionEnum.Developer);
        var salary = await new UserSalaryFake(user, professionOrNull: profession).PleaseAsync(context);

        var request = new EditSalaryRequest
        {
            Grade = DeveloperGrade.Middle,
            ProfessionId = salary.ProfessionId,
            Company = salary.Company,
        };

        Assert.NotEqual(request.Grade, salary.Grade);

        var result = await new SalariesController(
                new FakeAuth(user),
                context,
                new GlobalFake(),
                new Mock<IMediator>().Object)
            .Update(salary.Id, request, default);

        Assert.True(result.IsSuccess);
        Assert.Null(result.ErrorMessage);

        Assert.Equal(result.CreatedSalary.Value, salary.Value);
        Assert.Equal(result.CreatedSalary.Quarter, salary.Quarter);
        Assert.Equal(result.CreatedSalary.Year, salary.Year);
        Assert.Equal(result.CreatedSalary.Currency, salary.Currency);
        Assert.Equal(result.CreatedSalary.Company, salary.Company);
        Assert.Equal(result.CreatedSalary.Grade, salary.Grade);

        Assert.Equal(1, context.Salaries.Count());
    }

    [Fact]
    public async Task Update_HasNoRecordForNewQuarterAlready_UpdateByAdmin_Ok()
    {
        await using var context = new InMemoryDatabaseContext();
        var user = await new FakeUser(Role.Admin).PleaseAsync(context);
        var user1 = await new FakeUser(Role.Interviewer).PleaseAsync(context);
        var salary = await new UserSalaryFake(user1).PleaseAsync(context);

        var request = new CreateOrEditSalaryRecordRequest
        {
            Value = 600_000,
            Quarter = 3,
            Year = 2023,
            Currency = Currency.KZT,
            Company = CompanyType.Foreign,
            Grade = DeveloperGrade.Middle,
            ProfessionId = (long)UserProfessionEnum.ProductOwner,
        };

        Assert.NotEqual(request.Value, salary.Value);
        Assert.NotEqual(request.Quarter, salary.Quarter);
        Assert.NotEqual(request.Year, salary.Year);
        Assert.NotEqual(request.Company, salary.Company);
        Assert.NotEqual(request.Grade, salary.Grade);

        var result = await new SalariesController(
                new FakeAuth(user),
                context,
                new GlobalFake(),
                new Mock<IMediator>().Object)
            .Update(salary.Id, request, default);

        Assert.True(result.IsSuccess);
        Assert.Null(result.ErrorMessage);

        Assert.Equal(result.CreatedSalary.Value, salary.Value);
        Assert.Equal(result.CreatedSalary.Quarter, salary.Quarter);
        Assert.Equal(result.CreatedSalary.Year, salary.Year);
        Assert.Equal(result.CreatedSalary.Currency, salary.Currency);
        Assert.Equal(result.CreatedSalary.Company, salary.Company);
        Assert.Equal(result.CreatedSalary.Grade, salary.Grade);

        Assert.Equal(1, context.Salaries.Count());
    }

    [Fact]
    public async Task All_ReturnsAllData_Ok()
    {
        await using var context = new InMemoryDatabaseContext();
        var user = await new FakeUser(Role.Interviewer).PleaseAsync(context);

        var salary1 = await context.SaveAsync(new UserSalaryFake(
            user,
            value: 400_000,
            createdAt: DateTimeOffset.Now.AddYears(-1).AddDays(-1),
            professionOrNull: await context.Professions.FirstAsync(x => x.Id == (long)UserProfessionEnum.Developer))
            .AsDomain());

        var salary2 = await context.SaveAsync(new UserSalaryFake(
            user,
            value: 600_000,
            createdAt: DateTimeOffset.Now.AddDays(-1),
            professionOrNull: await context.Professions.FirstAsync(x => x.Id == (long)UserProfessionEnum.Developer))
            .AsDomain());

        var createdSalaries = context.Salaries.ToList();
        Assert.Equal(2, createdSalaries.Count);

        var salaries = await new SalariesController(
                new FakeAuth(user),
                context,
                new GlobalFake(),
                new Mock<IMediator>().Object)
            .AllAsync(
                new GetAllSalariesQueryParams(),
                default);

        Assert.Equal(2, salaries.TotalItems);
        Assert.Equal(2, salaries.Results.Count);
    }

    [Fact]
    public async Task Approve_SalaryWasNotApproved_Ok()
    {
        await using var context = new InMemoryDatabaseContext();
        var user1 = await new FakeUser(Role.Admin).PleaseAsync(context);

        var salary = await context.SaveAsync(new UserSalaryFake(
                user1,
                value: 400_000,
                useInStats: false)
            .AsDomain());

        Assert.False(salary.UseInStats);
        context.ChangeTracker.Clear();
        await new SalariesController(
                new FakeAuth(user1),
                context,
                new GlobalFake(),
                new Mock<IMediator>().Object)
            .Approve(salary.Id, default);

        salary = await context.Salaries.FirstOrDefaultAsync(x => x.Id == salary.Id);
        Assert.True(salary.UseInStats);
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

        await new SalariesController(
                new FakeAuth(user1),
                context,
                new GlobalFake(),
                new Mock<IMediator>().Object)
            .Delete(salary12.Id, default);

        allSalaries = context.Salaries.ToList();
        Assert.Single(allSalaries);
        Assert.Equal(salary11.Id, allSalaries[0].Id);
    }
}