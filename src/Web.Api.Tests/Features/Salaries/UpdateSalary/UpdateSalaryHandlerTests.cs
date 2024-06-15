using System.Linq;
using System.Threading.Tasks;
using Domain.Entities.Enums;
using Domain.Entities.Salaries;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;
using TestUtils.Auth;
using TestUtils.Db;
using TestUtils.Fakes;
using Web.Api.Features.Salaries.UpdateSalary;
using Xunit;

namespace Web.Api.Tests.Features.Salaries.UpdateSalary;

public class UpdateSalaryHandlerTests
{
    [Fact]
    public async Task Update_HasNoRecordForNewQuarterAlready_Ok()
    {
        await using var context = new InMemoryDatabaseContext();
        var user = await new FakeUser(Role.Interviewer).PleaseAsync(context);

        var profession = await context.Professions.FirstAsync(x => x.Id == (long)UserProfessionEnum.Developer);
        var salary = await new UserSalaryFake(user, professionOrNull: profession).PleaseAsync(context);

        var request = new UpdateSalaryCommand()
        {
            Id = salary.Id,
            Grade = DeveloperGrade.Middle,
            ProfessionId = salary.ProfessionId,
            Company = salary.Company,
        };

        Assert.NotEqual(request.Grade, salary.Grade);

        var result = await new UpdateSalaryHandler(
                context,
                new FakeAuth(user))
            .Handle(request, default);

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

        var request = new UpdateSalaryCommand
        {
            Id = salary.Id,
            Company = CompanyType.Foreign,
            Grade = DeveloperGrade.Middle,
            ProfessionId = (long)UserProfessionEnum.ProductOwner,
        };

        Assert.NotEqual(request.Company, salary.Company);
        Assert.NotEqual(request.Grade, salary.Grade);

        var result = await new UpdateSalaryHandler(
                context,
                new FakeAuth(user))
            .Handle(request, default);

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
}