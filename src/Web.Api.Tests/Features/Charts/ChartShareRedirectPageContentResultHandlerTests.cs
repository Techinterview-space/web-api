using TechInterviewer.Features.Charts;
using Xunit;

namespace Web.Api.Tests.Features.Charts;

public class ChartShareRedirectPageContentResultHandlerTests
{
    [Theory]
    [InlineData(1000, "1,000")]
    [InlineData(1_500_000, "1,500,000")]
    public void SalaryFormat_Cases_Match(
        double salary,
        string expected)
    {
        Assert.Equal(expected, ChartShareRedirectPageContentResultHandler.SalaryFormat(salary));
    }
}