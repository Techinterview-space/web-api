using System;
using Domain.Entities.Users;
using Domain.Services;

namespace Domain.Exceptions;

public class MismatchedIdentitiesException : InvalidOperationException
{
    public MismatchedIdentitiesException(User user, CurrentUser identityUser)
        : base($"User.Id:{user.Id} has mismatched identities.\r\n" +
               $"API.IdentityId:{user.IdentityId}\r\n" +
               $"SSO.IdentityId:{identityUser.Id}")
    {
    }
}