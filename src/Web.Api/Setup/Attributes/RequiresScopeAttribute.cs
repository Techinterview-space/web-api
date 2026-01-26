using System;
using System.Linq;
using Domain.Enums;
using Domain.Validation.Exceptions;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Web.Api.Setup.Attributes;

/// <summary>
/// Authorization attribute that validates M2M client scopes on endpoints.
/// This attribute only applies to M2M clients (machine-to-machine tokens).
/// Regular user tokens are not affected by this attribute.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class RequiresScopeAttribute : Attribute, IAuthorizationFilter
{
    private readonly string[] _requiredScopes;

    /// <summary>
    /// Creates a new RequiresScopeAttribute that requires one of the specified scopes.
    /// </summary>
    /// <param name="scopes">One or more scopes that satisfy the requirement (OR logic).</param>
    public RequiresScopeAttribute(params string[] scopes)
    {
        _requiredScopes = scopes ?? throw new ArgumentNullException(nameof(scopes));

        if (_requiredScopes.Length == 0)
        {
            throw new ArgumentException("At least one scope must be specified", nameof(scopes));
        }
    }

    public void OnAuthorization(AuthorizationFilterContext context)
    {
        var user = context.HttpContext.User;

        // Only check scopes for M2M tokens (identified by token_type claim)
        var tokenType = user.FindFirst("token_type")?.Value;
        if (tokenType != "m2m")
        {
            // Regular user tokens bypass scope checks
            return;
        }

        // Extract scopes from claims
        var userScopes = user.FindAll("scope")
            .Select(c => c.Value)
            .ToList();

        // Full access bypasses all scope checks
        if (userScopes.Contains(M2mScope.FullAccess))
        {
            return;
        }

        // Check if any of the required scopes are present
        var hasRequiredScope = _requiredScopes.Any(required => userScopes.Contains(required));

        if (!hasRequiredScope)
        {
            throw new NoPermissionsException(
                $"M2M client does not have required scope. Required one of: {string.Join(", ", _requiredScopes)}");
        }
    }
}
