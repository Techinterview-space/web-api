using Domain.Entities.Users;
using Domain.Enums;
using Domain.Services;

namespace Infrastructure.Authentication.Contracts;

public interface IAuthorization
{
    Task<User> CurrentUserOrFailAsync();

    Task<User> CurrentUserOrNullAsync();

    CurrentUser CurrentUser { get; }

    Task HasRoleOrFailAsync(Role role);

    Task HasAnyRoleOrFailAsync(params Role[] roles);
}