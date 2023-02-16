using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Domain.Enums;
using Domain.Services;
using MG.Utils.Abstract;
using MG.Utils.Abstract.NonNullableObjects;
using MG.Utils.Entities;
using MG.Utils.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace Domain.Entities.Users;

public static class UserExtensions
{
    public static async Task<User> ByEmailOrNullAsync(this IQueryable<User> query, string email)
    {
        if (string.IsNullOrEmpty(email))
        {
            throw new ArgumentNullException(paramName: nameof(email));
        }

        var emailUpper = email.ToUpperInvariant();
        return await query
            .FirstOrDefaultAsync(x => x.Email.ToUpper() == emailUpper);
    }

    public static bool Has(this User user, Role role)
    {
        return user.UserRoles.Any(x => x.RoleId == role);
    }

    public static void HasOrFail(this User user, Role role, string message = null)
    {
        if (user.Has(role))
        {
            return;
        }

        throw Exception(message);
    }

    public static void HasOrFail(this CurrentUser user, Role role, string message = null)
    {
        if (user.Has(role))
        {
            return;
        }

        throw Exception(message);
    }

    public static void HasAnyOrFail(this User user, params Role[] roles) => HasAnyOrFail(user, roles as IReadOnlyCollection<Role>);

    public static void HasAnyOrFail(this User user, IReadOnlyCollection<Role> roles)
    {
        roles.ThrowIfNullOrEmpty(nameof(roles));

        if (roles.Any(user.Has))
        {
            return;
        }

        throw Exception(null);
    }

    private static NoPermissionsException Exception(string messageOrNull)
    {
        messageOrNull ??= "Current user has no permission to do this operation";
        return new NoPermissionsException(messageOrNull);
    }

    public static User InactiveOrFail(this User user)
    {
        if (!user.Active())
        {
            return user;
        }

        throw new InvalidOperationException($"The user Id:{user.Id} is active");
    }
}