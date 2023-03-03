using Domain.Authentication.Abstract;
using Domain.Services;
using Domain.Validation;

namespace TestUtils.Auth;

public class FakeHttpContext : IHttpContext
{
    public FakeHttpContext()
    {
        Exists = false;
        CurrentUser = null;
    }

    public FakeHttpContext(CurrentUser currentUser)
    {
        currentUser.ThrowIfNull(nameof(currentUser));
        Exists = true;
        CurrentUser = currentUser;
    }

    public CurrentUser CurrentUser { get; }

    public bool Exists { get; }
}