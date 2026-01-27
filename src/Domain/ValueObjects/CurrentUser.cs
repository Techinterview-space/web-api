using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using Domain.Enums;
using Domain.Extensions;
using Domain.Validation;

namespace Domain.ValueObjects;

public record CurrentUser
{
    public const string GoogleOAuth2Prefix = "google-oauth2|";
    public const string GooglePrefix = "google|";
    public const string GithubPrefix = "github|";
    public const string Auth0Prefix = "auth0|";
    public const string LocalPrefix = "local|";
    public const string M2mPrefix = "m2m|";

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

        UserId = principal.GetClaimValue(ClaimTypes.NameIdentifier, false)
            ?? principal.GetClaimValue(JwtRegisteredClaimNames.Sub);
        Email = principal.GetClaimValue(ClaimTypes.Email, false) ?? principal.GetClaimValue("email");
        IsEmailVerified = principal.GetClaimValue("email_verified", false) == "true";
        FirstName = principal.GetClaimValue(ClaimTypes.GivenName, false);
        LastName = principal.GetClaimValue(ClaimTypes.Surname, false);
        ProfilePicture = principal.GetClaimValue("picture", false);

        if (LastName is null && FirstName is null)
        {
            var fullname = principal.GetClaimValue(ClaimTypes.Name, false);
            if (fullname != null)
            {
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
        }

        Roles = principal.Claims
            .Where(x => x.Type == ClaimTypes.Role)
            .Select(x => x.Value.ToEnum<Role>())
            .ToArray();

        Scopes = principal.Claims
            .Where(x => x.Type == "scope")
            .Select(x => x.Value)
            .ToArray();
    }

    /// <summary>
    /// User identity ID with provider prefix (e.g., 'google|12345', 'github|789', 'local|abc', 'm2m|clientid').
    /// </summary>
    public string UserId { get; protected set; }

    public string Email { get; protected set; }

    public bool IsEmailVerified { get; protected set; }

    public string FirstName { get; protected set; }

    public string LastName { get; protected set; }

    /// <summary>
    /// Claims picture.
    /// </summary>
    public string ProfilePicture { get; protected set; }

    public IReadOnlyCollection<Role> Roles { get; protected set; }

    /// <summary>
    /// M2M client scopes (for machine-to-machine authentication).
    /// </summary>
    public IReadOnlyCollection<string> Scopes { get; protected set; } = Array.Empty<string>();

    public bool Has(Role role)
    {
        return Roles.Contains(role);
    }

    public void HasOrFail(Role role)
    {
        if (!Has(role))
        {
            throw new UnauthorizedAccessException($"User does not have {role} role");
        }
    }

    public bool HasAny(IReadOnlyCollection<Role> roles)
    {
        roles.ThrowIfNullOrEmpty(nameof(roles));
        return roles.Any(Roles.Contains);
    }

    public string Fullname => $"{FirstName} {LastName}";

    public string EmailUpper => Email?.ToUpperInvariant();

    public bool IsGoogleAuth()
    {
        return UserId != null &&
               (UserId.StartsWith(GoogleOAuth2Prefix) || UserId.StartsWith(GooglePrefix));
    }

    public bool IsGithubAuth()
    {
        return UserId != null &&
               UserId.StartsWith(GithubPrefix);
    }

    public bool IsAuth0Auth()
    {
        return UserId != null &&
               UserId.StartsWith(Auth0Prefix);
    }

    public bool IsLocalAuth()
    {
        return UserId != null &&
               UserId.StartsWith(LocalPrefix);
    }

    public bool IsM2mClient()
    {
        return UserId != null &&
               UserId.StartsWith(M2mPrefix);
    }

    /// <summary>
    /// Gets the M2M client ID without the prefix.
    /// </summary>
    public string M2mClientId => IsM2mClient() ? UserId[M2mPrefix.Length..] : null;

    /// <summary>
    /// Checks if the current user (M2M client) has the specified scope.
    /// </summary>
    public bool HasScope(string scope)
    {
        return Scopes.Contains("*") || Scopes.Contains(scope);
    }
}