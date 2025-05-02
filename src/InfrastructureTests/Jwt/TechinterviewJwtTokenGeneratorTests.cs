using Infrastructure.Jwt;
using Xunit;

namespace InfrastructureTests.Jwt;

public class TechinterviewJwtTokenGeneratorTests
{
    [Fact]
    public void ToString_ShouldReturnToken()
    {
        // Arrange
        var secretKey = "eV)1T_9>zm7|";
        var generator = new TechinterviewJwtTokenGenerator(secretKey);

        var result = generator.ToString();
        Assert.Contains("eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9", result);
    }
}