using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Domain.Database;
using Domain.Entities.Users;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;
using TestUtils.Db;

namespace TestUtils.Fakes;

public class FakeUser : User, IPlease<User>
{
    public FakeUser(
        Role role,
        string firstName = null,
        string lastName = null,
        string userName = null,
        bool emailConfirmed = false)
    {
        FirstName = firstName ?? Faker.Name.First();
        LastName = lastName ?? Faker.Name.Last();

        userName ??= $"{FirstName.First()}.{LastName}_{DateTimeOffset.Now.Ticks}@example.com".ToLowerInvariant();
        Email = userName;
        EmailConfirmed = emailConfirmed;
        UserRoles = new List<UserRole>
        {
            new UserRole(role, this)
        };
    }

    public async Task<User> PleaseAsync(DatabaseContext context)
    {
        var userEntry = await context.Users.AddAsync(Please());
        await context.TrySaveChangesAsync();
        return await context.Users
            .Include(x => x.UserRoles)
            .ByIdOrFailAsync(userEntry.Entity.Id);
    }

    public User Please()
    {
        return this;
    }

    public IPlease<User> AsPlease()
    {
        return this;
    }
}