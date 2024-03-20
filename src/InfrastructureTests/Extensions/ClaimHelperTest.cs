using System;
using System.Collections.Generic;
using System.Security.Claims;
using Domain.Extensions;
using Xunit;

namespace InfrastructureTests.Extensions;

public class ClaimHelperTest
{
    [Fact]
    public void ClaimsForException_NoClaims()
    {
        var target = CreateTestInstance(new List<Claim>());
        Assert.Equal(string.Empty, target.Claims.ClaimsForException());
    }

    [Fact]
    public void ClaimsForException_SeveralClaims()
    {
        var target = CreateTestInstance(new List<Claim>
        {
            new Claim(ClaimTypes.Email, "j.smith@petrel.ai"),
            new Claim(ClaimTypes.GivenName, "John"),
            new Claim(ClaimTypes.Surname, "Smith")
        });

        Assert.Equal(
            $"{ClaimTypes.Email}: j.smith@petrel.ai\r\n" +
            $"{ClaimTypes.GivenName}: John\r\n" +
            $"{ClaimTypes.Surname}: Smith\r\n",
            target.Claims.ClaimsForException());
    }

    [Fact]
    public void GetClaimValue_ClaimsNotNull_TypeExists_Ok()
    {
        ClaimsPrincipal principal = CreateTestInstance(new List<Claim>
        {
            new Claim(ClaimTypes.Email, "john_smith@email.com"),
            new Claim(ClaimTypes.GivenName, "john"),
            new Claim(ClaimTypes.Surname, "smith")
        });

        Assert.Equal("john_smith@email.com", principal.GetClaimValue(ClaimTypes.Email));
        Assert.Equal("john", principal.GetClaimValue(ClaimTypes.GivenName));
        Assert.Equal("smith", principal.GetClaimValue(ClaimTypes.Surname));

        Assert.Equal("john_smith@email.com", principal.Claims.GetClaimValue(ClaimTypes.Email));
        Assert.Equal("john", principal.Claims.GetClaimValue(ClaimTypes.GivenName));
        Assert.Equal("smith", principal.Claims.GetClaimValue(ClaimTypes.Surname));
    }

    [Fact]
    public void GetClaimValue_ClaimsNotNull_TypeDoesNotExist_ThrowIfError_Exception()
    {
        ClaimsPrincipal principal = CreateTestInstance(new List<Claim>
        {
            new Claim(ClaimTypes.Email, "john_smith@email.com"),
            new Claim(ClaimTypes.GivenName, "john")
        });

        Assert.Throws<InvalidOperationException>(() => principal.GetClaimValue(ClaimTypes.Surname));
        Assert.Throws<InvalidOperationException>(() => principal.Claims.GetClaimValue(ClaimTypes.Surname));
    }

    [Fact]
    public void GetClaimValue_ClaimsNotNull_TypeDoesNotExist_NotThrowIfError_Ok()
    {
        ClaimsPrincipal principal = CreateTestInstance(new List<Claim>
        {
            new Claim(ClaimTypes.Email, "john_smith@email.com"),
            new Claim(ClaimTypes.GivenName, "john")
        });

        Assert.Null(principal.GetClaimValue(ClaimTypes.Surname, false));
        Assert.Null(principal.Claims.GetClaimValue(ClaimTypes.Surname, false));
    }

    [Fact]
    public void GetClaimValue_ClaimsAreEmpty_TypeDoesNotExist_NotThrowIfError_Ok()
    {
        ClaimsPrincipal principal = CreateTestInstance(new List<Claim>());

        Assert.Null(principal.GetClaimValue(ClaimTypes.Surname, false));
        Assert.Null(principal.Claims.GetClaimValue(ClaimTypes.Surname, false));
    }

    [Fact]
    public void GetClaimValue_ClaimsAreEmpty_TypeDoesNotExist_ThrowIfError_Exception()
    {
        ClaimsPrincipal principal = CreateTestInstance(new List<Claim>());
        Assert.Throws<InvalidOperationException>(() => principal.GetClaimValue(ClaimTypes.Surname));
        Assert.Throws<InvalidOperationException>(() => principal.Claims.GetClaimValue(ClaimTypes.Surname));
    }

    [Fact]
    public void GetClaimValue_ClaimsNull_Exception()
    {
        List<Claim> claims = null;
        Assert.Throws<ArgumentNullException>(() => claims.GetClaimValue(ClaimTypes.Surname));
    }

    [Fact]
    public void GetClaimValue_TypeNull_Exception()
    {
        ClaimsPrincipal principal = CreateTestInstance(new List<Claim>());
        Assert.Throws<ArgumentNullException>(() => principal.GetClaimValue(null));
        Assert.Throws<ArgumentNullException>(() => principal.Claims.GetClaimValue(null));
    }

    private ClaimsPrincipal CreateTestInstance(IEnumerable<Claim> claims)
    {
        return new ClaimsPrincipal(new ClaimsIdentity(claims));
    }
}