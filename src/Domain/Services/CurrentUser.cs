using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using Domain.Enums;
using Domain.Extensions;
using Domain.Validation;

namespace Domain.Services;

public record CurrentUser
{
    // For test purposes
    protected CurrentUser()
    {
    }

    public CurrentUser(
        ClaimsPrincipal principal)
    {
        principal.ThrowIfNull(nameof(principal));

        if (!principal.HasClaims())
        {
            throw new ArgumentException("Principal does not have any claim");
        }

        Id = principal.GetClaimValue(ClaimTypes.NameIdentifier);
        Email = principal.GetClaimValue(ClaimTypes.Email);
        FirstName = principal.GetClaimValue(ClaimTypes.GivenName, false);
        LastName = principal.GetClaimValue(ClaimTypes.Surname, false);

        if (LastName is null && FirstName is null)
        {
            var fullname = principal.GetClaimValue(ClaimTypes.Name);
            var names = fullname.Split(' ');
            if (names.Length == 1)
            {
                FirstName = names[0];
                LastName = "-";
            }
            else
            {
                FirstName = names[0];
                LastName = names[1];
            }
        }

        Roles = principal.Claims
            .Where(x => x.Type == ClaimTypes.Role)
            .Select(x => x.Value.ToEnum<Role>())
            .ToArray();
    }

    public string Id { get; protected set; }

    public string Email { get; protected set; }

    public string FirstName { get; protected set; }

    public string LastName { get; protected set; }

    public IReadOnlyCollection<Role> Roles { get; protected set; }

    public bool Has(Role role)
    {
        return Roles.Contains(role);
    }

    public bool HasAny(IReadOnlyCollection<Role> roles)
    {
        roles.ThrowIfNullOrEmpty(nameof(roles));
        return roles.Any(Roles.Contains);
    }

    public string Fullname => $"{FirstName} {LastName}";
}