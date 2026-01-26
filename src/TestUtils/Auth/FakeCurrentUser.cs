using System;
using System.Collections.Generic;
using System.Linq;
using Domain.Entities.Users;
using Domain.Enums;
using Domain.ValueObjects;

namespace TestUtils.Auth;

public record FakeCurrentUser : CurrentUser
{
    public FakeCurrentUser(
        User user)
    {
        UserId = user.IdentityId ?? $"google-oauth2|{Guid.NewGuid():N}";
        FirstName = user.FirstName;
        LastName = user.LastName;
        Email = user.Email;
        Roles = user.UserRoles.Select(x => x.RoleId).ToArray();
    }

    public FakeCurrentUser(
        string userId = "1",
        Role role = Role.Interviewer,
        string firstName = null,
        string lastName = null,
        string email = null,
        bool isEmailVerified = true)
    {
        UserId = userId;
        FirstName = firstName ?? Faker.Name.First();
        LastName = lastName ?? Faker.Name.Last();

        email ??= $"{FirstName.First()}.{LastName}_{DateTimeOffset.Now.Ticks}@example.com".ToLowerInvariant();
        Email = email;
        IsEmailVerified = isEmailVerified;
        Roles = new List<Role>
        {
            role
        };
    }

    public FakeCurrentUser WithUserId(
        string userId)
    {
        UserId = userId;
        return this;
    }

    public FakeCurrentUser WithEmail(
        string email)
    {
        Email = email;
        return this;
    }
}