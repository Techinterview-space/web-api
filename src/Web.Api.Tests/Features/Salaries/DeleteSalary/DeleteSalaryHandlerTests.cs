using System;
using System.Linq;
using System.Threading.Tasks;
using Domain.Enums;
using TestUtils.Db;
using TestUtils.Fakes;
using Web.Api.Features.Salaries.DeleteSalary;
using Xunit;

namespace Web.Api.Tests.Features.Salaries.DeleteSalary;

public class DeleteSalaryHandlerTests
{
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

        await new DeleteSalaryHandler(context)
            .Handle(new DeleteSalaryCommand(salary12.Id), default);

        allSalaries = context.Salaries.ToList();
        Assert.Single(allSalaries);
        Assert.Equal(salary11.Id, allSalaries[0].Id);
    }
}