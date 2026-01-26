using Domain.ValueObjects;
using Xunit;

namespace Domain.Tests.ValueObjects;

public class CompanySlugTests
{
    [Theory]
    [InlineData("Halyk Bank", "halyk-bank-")]
    [InlineData("Kaspi.kz", "kaspi-kz-")]
    [InlineData("Airba fresh", "airba-fresh-")]
    public void Ctor_Cases_Match(
        string source,
        string expectedStart)
    {
        var slug = new CompanySlug(source);
        var result = slug.ToString();

        Assert.StartsWith(expectedStart, result);
    }
}