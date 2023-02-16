using System;
using System.ComponentModel.DataAnnotations.Schema;
using Domain.Entities.Users;
using MG.Utils.Abstract.Entities;
using MG.Utils.Entities;

namespace Domain.Entities.Organizations;

public class OrganizationUser : HasDatesBase, IHasIdBase<long>
{
    protected OrganizationUser()
    {
    }

    public OrganizationUser(
        User user,
        Organization organization)
    {
        UserId = user.Id;
        OrganizationId = organization.Id;
    }

    public long Id { get; protected set; }

    public long UserId { get; protected set; }

    public virtual User User { get; protected set; }

    public Guid OrganizationId { get; protected set; }

    public virtual Organization Organization { get; protected set; }

    public DateTimeOffset? DeletedAt { get; protected set; }

    [NotMapped]
    public bool Active => DeletedAt == null;

    public void Delete()
    {
        if (DeletedAt != null)
        {
            throw new InvalidOperationException("Organization is already deleted");
        }

        DeletedAt = DateTimeOffset.Now;
    }
}