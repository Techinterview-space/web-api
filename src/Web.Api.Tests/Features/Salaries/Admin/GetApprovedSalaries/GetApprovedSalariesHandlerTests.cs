using System;
using System.Linq;
using System.Threading.Tasks;
using Domain.Entities.Salaries;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;
using TestUtils.Db;
using TestUtils.Fakes;
using Web.Api.Features.Salaries.Admin.GetApprovedSalaries;
using Xunit;

namespace Web.Api.Tests.Features.Salaries.Admin.GetApprovedSalaries;

public class GetApprovedSalariesHandlerTests
{
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

        var salaries = await new GetApprovedSalariesHandler(context)
            .Handle(
                new GetApprovedSalariesQuery(),
                default);

        Assert.Equal(2, salaries.TotalItems);
        Assert.Equal(2, salaries.Results.Count);
    }
}