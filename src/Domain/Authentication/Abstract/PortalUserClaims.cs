using System.Collections.Generic;
using System.Security.Claims;
using Domain.Entities.Users;
using MG.Utils.Abstract;

namespace Domain.Authentication.Abstract;

public class PortalUserClaims
{
    private readonly User _user;

    public PortalUserClaims(User user)
    {
        user.ThrowIfNull(nameof(user));
        _user = user;
    }

    public ClaimsPrincipal Principal()
    {
        return new ClaimsPrincipal(Identity());
    }

    public ClaimsIdentity Identity()
    {
        return new ClaimsIdentity(
            claims: Claims(),
            authenticationType: "ApplicationCookie",
            nameType: ClaimsIdentity.DefaultNameClaimType,
            roleType: ClaimsIdentity.DefaultRoleClaimType);
    }

    private IEnumerable<Claim> Claims()
    {
        yield return new Claim(ClaimsIdentity.DefaultNameClaimType, _user.Email);
        yield return new Claim(ClaimTypes.Email, _user.Email);
        yield return new Claim(ClaimTypes.NameIdentifier, _user.Id.ToString());
        yield return new Claim(ClaimTypes.GivenName, _user.FirstName);
        yield return new Claim(ClaimTypes.Surname, _user.LastName);

        foreach (UserRole role in _user.UserRoles)
        {
            yield return new Claim(ClaimTypes.Role, role.RoleId.ToString());
        }
    }
}