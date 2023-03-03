using Domain.Validation;
using Xunit;

namespace DomainTests.Validation;

public class EmailValidRegexTest
{
    [Theory]
    [InlineData("@sd.com", false)]
    [InlineData("@sdcom", false)]
    [InlineData("js@contoso.中国", true)]
    [InlineData("j.s@server1.proseware.com", true)]
    [InlineData("j_9@[129.126.118.1]", true)]
    [InlineData("j..s@proseware.com", false)]
    [InlineData("js*@proseware.com", false)]
    [InlineData("js@proseware..com", false)]
    [InlineData("j.@server1.proseware.com", false)]
    [InlineData("j.smith@gmail.com", true)]
    [InlineData("j.smith@petrel.ai", true)]
    public void EmailValidationTest(string email, bool result)
    {
        Assert.Equal(result, new EmailValidRegex().IsValid(email));
    }
}