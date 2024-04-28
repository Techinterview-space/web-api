using Domain.Entities.Users;
using Domain.Enums;
using Domain.ValueObjects;

namespace Infrastructure.Authentication.Contracts;

public interface IAuthorization
{
    Task<User> CurrentUserOrFailAsync(
        CancellationToken cancellationToken = default);

    Task<User> CurrentUserOrNullAsync(
        CancellationToken cancellationToken = default);

    CurrentUser CurrentUser { get; }

    Task HasRoleOrFailAsync(
        Role role,
        CancellationToken cancellationToken = default);

    Task HasAnyRoleOrFailAsync(params Role[] roles);
}