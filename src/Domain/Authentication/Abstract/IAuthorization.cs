using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Domain.Entities.Users;
using Domain.Enums;
using Domain.Services;

namespace Domain.Authentication.Abstract;

public interface IAuthorization
{
    Task<User> CurrentUserOrFailAsync();

    Task<User> CurrentUserOrNullAsync();

    CurrentUser CurrentUser { get; }

    Task HasRoleOrFailAsync(Role role);

    Task HasAnyRoleOrFailAsync(params Role[] roles);
}