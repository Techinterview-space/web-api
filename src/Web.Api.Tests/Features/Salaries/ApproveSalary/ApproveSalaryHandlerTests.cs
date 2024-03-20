using System.Threading.Tasks;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;
using TechInterviewer.Features.Salaries.ApproveSalary;
using TestUtils.Db;
using TestUtils.Fakes;
using Xunit;

namespace Web.Api.Tests.Features.Salaries.ApproveSalary;

public class ApproveSalaryHandlerTests
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
        await new ApproveSalaryHandler(
                context)
            .Handle(new ApproveSalaryCommand(salary.Id), default);

        salary = await context.Salaries.FirstOrDefaultAsync(x => x.Id == salary.Id);
        Assert.True(salary.UseInStats);
    }
}