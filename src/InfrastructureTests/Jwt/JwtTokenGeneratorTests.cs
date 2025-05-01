using Infrastructure.Jwt;
using Xunit;

namespace InfrastructureTests.Jwt;

public class JwtTokenGeneratorTests
{
    [Fact]
    public void ToString_ShouldReturnToken()
    {
        // Arrange
        var secretKey = "my-secret-phrase-please-do-not-use-this-in-production";
        var generator = new JwtTokenGenerator(secretKey);

        var result = generator.ToString();
        Assert.Contains("eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9", result);
    }
}