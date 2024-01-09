using System.Linq;
using System.Threading.Tasks;
using Domain.Entities.Enums;
using Domain.Entities.Salaries;
using Domain.Enums;
using TechInterviewer.Controllers.Organizations;
using TechInterviewer.Controllers.Salaries;
using TestUtils;
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
}