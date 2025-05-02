using Infrastructure.Jwt;
using Xunit;

namespace InfrastructureTests.Jwt;

public class TechinterviewJwtTokenGeneratorTests
{
    [Fact]
    public void ToString_ShouldReturnToken()
    {
        const string secretKey = "hey-girl-its-super-secret-key-for-jwt";
        var generator = new TechinterviewJwtTokenGenerator(secretKey);

        var result = generator.ToString();
        Assert.Contains("eyJhbGciOiJodHRwOi8vd3d3LnczLm9yZy8y", result);
    }
}