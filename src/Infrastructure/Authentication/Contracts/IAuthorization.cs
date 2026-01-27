using Domain.Entities.Users;
using Domain.Enums;
using Domain.ValueObjects;

namespace Infrastructure.Authentication.Contracts;

public interface IAuthorization
{
    Task<User> GetCurrentUserOrFailAsync(
        CancellationToken cancellationToken = default);

    Task<User> GetCurrentUserOrNullAsync(
        CancellationToken cancellationToken = default);

    CurrentUser CurrentUser { get; }

    bool HasUserClaims { get; }

    Task HasRoleOrFailAsync(
        Role role,
        CancellationToken cancellationToken = default);

    Task HasAnyRoleOrFailAsync(
        params Role[] roles);

    Task<User> GetOrCreateAsync(
        CancellationToken cancellationToken);
}