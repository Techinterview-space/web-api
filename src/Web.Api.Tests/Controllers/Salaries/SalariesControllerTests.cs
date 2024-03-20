using System;
using System.Linq;
using System.Threading.Tasks;
using Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Moq;
using TechInterviewer.Features.Salaries;
using TestUtils.Db;
using TestUtils.Fakes;
using Xunit;

namespace Web.Api.Tests.Controllers.Salaries;

public class SalariesControllerTests
{
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
                context,
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
                context,
                new Mock<IMediator>().Object)
            .Delete(salary12.Id, default);

        allSalaries = context.Salaries.ToList();
        Assert.Single(allSalaries);
        Assert.Equal(salary11.Id, allSalaries[0].Id);
    }
}