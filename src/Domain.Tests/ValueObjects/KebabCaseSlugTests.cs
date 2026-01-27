using Domain.ValueObjects;
using Xunit;

namespace Domain.Tests.ValueObjects;

public class KebabCaseSlugTests
{
    [Theory]
    [InlineData("Hello (World)", "hello-world")]
    [InlineData("Hello \"World\"", "hello-world")]
    [InlineData("Hello.World", "hello-world")]
    [InlineData("Hello World", "hello-world")]
    [InlineData("HelloWorld", "hello-world")]
    [InlineData("Hello_World", "hello-world")]
    [InlineData("Hello  World", "hello-world")]
    [InlineData("Hello__World", "hello-world")]
    [InlineData("Hello___World", "hello-world")]
    [InlineData("Hello_______World", "hello-world")]
    [InlineData("Hello-World", "hello-world")]
    [InlineData("Hello-World-", "hello-world")]
    [InlineData("-Hello-World-", "hello-world")]
    [InlineData("-Hello-World", "hello-world")]
    [InlineData("--Hello-World", "hello-world")]
    [InlineData(" __- - -some_randomStringExample __  - -- ", "some-random-string-example")]
    public void Ctor_Cases_Match(
        string source,
        string expected)
    {
        var slug = new KebabCaseSlug(source);
        Assert.Equal(expected, slug.ToString());
    }
}