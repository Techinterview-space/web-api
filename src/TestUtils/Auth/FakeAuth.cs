using System;
using System.Threading;
using System.Threading.Tasks;
using Domain.Entities.Users;
using Domain.Enums;
using Domain.ValueObjects;
using Infrastructure.Authentication.Contracts;

namespace TestUtils.Auth;

public class FakeAuth : IAuthorization
{
    private readonly User _user;

    public FakeAuth(User user)
    {
        _user = user;
    }

    public Task<User> GetCurrentUserOrFailAsync(
        CancellationToken cancellationToken = default)
    {
        return GetCurrentUserOrNullAsync(cancellationToken);
    }

    public Task<User> GetCurrentUserOrNullAsync(
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_user);
    }

    public CurrentUser CurrentUser
        => new FakeCurrentUser(_user);

    public bool HasUserClaims
        => true;

    public Task HasRoleOrFailAsync(
        Role role,
        CancellationToken cancellationToken = default)
    {
        _user.HasOrFail(role);
        return Task.CompletedTask;
    }

    public Task HasAnyRoleOrFailAsync(params Role[] roles)
    {
        _user.HasAnyOrFail(roles);
        return Task.CompletedTask;
    }

    public Task<User> GetOrCreateAsync(
        CancellationToken cancellationToken)
    {
        throw new NotSupportedException();
    }
}