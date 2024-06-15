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
        Id = user.Id.ToString();
        FirstName = user.FirstName;
        LastName = user.LastName;
        Email = user.Email;
        Roles = user.UserRoles.Select(x => x.RoleId).ToArray();
    }

    public FakeCurrentUser(
        string id = "1",
        Role role = Role.Interviewer,
        string firstName = null,
        string lastName = null,
        string email = null,
        bool isEmailVerified = true)
    {
        Id = id;
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
}