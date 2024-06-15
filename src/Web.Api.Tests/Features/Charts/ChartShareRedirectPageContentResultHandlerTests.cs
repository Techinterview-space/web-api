using Web.Api.Features.SalariesChartShare.GetChartSharePage;
using Xunit;

namespace Web.Api.Tests.Features.Charts;

public class GetChartSharePageHandlerTests
{
    [Theory]
    [InlineData(1000, "1,000")]
    [InlineData(1_500_000, "1,500,000")]
    public void SalaryFormat_Cases_Match(
        double salary,
        string expected)
    {
        Assert.Equal(expected, GetChartSharePageHandler.SalaryFormat(salary));
    }
}