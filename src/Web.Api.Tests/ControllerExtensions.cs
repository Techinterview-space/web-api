using System;
using System.Linq;
using System.Reflection;
using Domain.Enums;
using Microsoft.AspNetCore.Mvc;
using Web.Api.Setup.Attributes;

namespace Web.Api.Tests;

public static class ControllerExtensions
{
    public static bool HasRole<T>(this T controller, string actionName, Role roleToCheck)
        where T : ControllerBase
    {
        var controllerType = controller.GetType();
        var method = controllerType.GetMethod(actionName);

        if (method?.GetCustomAttribute(typeof(HasAnyRoleAttribute), true) is not HasAnyRoleAttribute attribute)
        {
            throw new NullReferenceException($"{nameof(attribute)} cannot be null");
        }

        return attribute.RolesToCheck.Contains(roleToCheck);
    }
}