using System;
using System.Threading.Tasks;
using Domain.Database;
using Domain.Entities.Organizations;
using Domain.Entities.Users;
using Microsoft.EntityFrameworkCore;
using TestUtils.Db;

namespace TestUtils.Fakes;

public class FakeOrganization : Organization, IPlease<Organization>
{
    public FakeOrganization(
        User managerOrNull = null)
    : base(
        "Awesome organization",
        "This organization is perfect",
        managerOrNull)
    {
    }

    public FakeOrganization MakeInactive()
    {
        DeletedAt = DateTimeOffset.Now;
        return this;
    }

    public FakeOrganization WithUser(
        User user)
    {
        Users.Add(new OrganizationUser(user, this));
        return this;
    }

    public FakeOrganization WithInvitation(
        User otherPerson,
        User currentUser)
    {
        Invitations.Add(new JoinToOrgInvitation(
            this,
            otherPerson,
            currentUser));
        return this;
    }

    public Organization Please()
    {
        return this;
    }

    public IPlease<Organization> AsPlease()
    {
        return this;
    }

    public async Task<Organization> PleaseAsync(DatabaseContext context)
    {
        var entry = await context.Organizations.AddAsync(Please());
        await context.TrySaveChangesAsync();
        return await context.Organizations
            .Include(x => x.Users)
            .ThenInclude(x => x.User)
            .ByIdOrFailAsync(entry.Entity.Id);
    }
}