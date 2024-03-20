using System.Threading.Tasks;
using Domain.Entities.Users;
using Domain.Enums;
using Domain.Services;
using Infrastructure.Authentication.Contracts;

namespace TestUtils.Auth;

public class FakeAuth : IAuthorization
{
    private readonly User _user;

    public FakeAuth(User user)
    {
        _user = user;
    }

    public Task<User> CurrentUserOrFailAsync()
    {
        return CurrentUserOrNullAsync();
    }

    public Task<User> CurrentUserOrNullAsync()
    {
        return Task.FromResult(_user);
    }

    public CurrentUser CurrentUser => new FakeCurrentUser(_user);

    public Task HasRoleOrFailAsync(Role role)
    {
        _user.HasOrFail(role);
        return Task.CompletedTask;
    }

    public Task HasAnyRoleOrFailAsync(params Role[] roles)
    {
        _user.HasAnyOrFail(roles);
        return Task.CompletedTask;
    }
}