using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Authentication;
using Domain.Enums;
using Domain.Validation.Exceptions;
using Domain.ValueObjects;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Web.Api.Setup.Attributes;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
public class HasAnyRoleAttribute : Attribute, IAuthorizationFilter
{
    public IReadOnlyCollection<Role> RolesToCheck { get; }

    public HasAnyRoleAttribute(params Role[] rolesToCheck)
    {
        RolesToCheck = rolesToCheck;
    }

    public void OnAuthorization(AuthorizationFilterContext context)
    {
        var hasAuth = context.HttpContext.User.Claims.Any();

        if (!hasAuth)
        {
            throw new AuthenticationException("You have to be authorized to execute the operation");
        }

        if (!RolesToCheck.Any() || new CurrentUser(context.HttpContext.User).HasAny(RolesToCheck))
        {
            return;
        }

        throw new NoPermissionsException("You are not allowed to interact with this action");
    }
}