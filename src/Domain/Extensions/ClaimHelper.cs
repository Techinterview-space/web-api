using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;

namespace Domain.Extensions;

public static class ClaimHelper
{
    public static bool HasClaims(this ClaimsPrincipal principal)
    {
        return principal.Claims.HasClaims();
    }

    public static bool HasClaims(this IEnumerable<Claim> claims)
    {
        return claims.Any();
    }

    public static string GetClaimValue(this ClaimsPrincipal principal, string type, bool throwExIfNotFound = true)
    {
        if (principal == null)
        {
            throw new ArgumentNullException(paramName: nameof(principal));
        }

        return principal.Claims.GetClaimValue(type, throwExIfNotFound);
    }

    public static string GetClaimValue(this IEnumerable<Claim> claims, string type, bool throwExIfNotFound = true)
    {
        if (string.IsNullOrEmpty(type))
        {
            throw new ArgumentNullException(paramName: nameof(type));
        }

        if (claims == null)
        {
            throw new ArgumentNullException(paramName: nameof(claims));
        }

        Claim claim = claims.FirstOrDefault(x => x.Type == type);

        if (claim == null && throwExIfNotFound)
        {
            throw new InvalidOperationException(
                $"Cannot find claim value for type '{type}'\r\n" +
                $"Claims:\r\n\r\n{ClaimsForException(claims)}");
        }

        return claim?.Value;
    }

    public static string ClaimsForException(this IEnumerable<Claim> claims)
    {
        return claims.Aggregate(
            string.Empty, (seed, c) => seed + c.ToString() + "\r\n");
    }
}