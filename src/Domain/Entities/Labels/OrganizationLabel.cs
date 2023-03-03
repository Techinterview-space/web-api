using System;
using System.Collections.Generic;
using Domain.Entities.Employments;
using Domain.Entities.Organizations;
using Domain.Entities.Users;
using Domain.Enums;
using Domain.Exceptions;
using Domain.ValueObjects;

namespace Domain.Entities.Labels;

public class OrganizationLabel : EntityLabelBase
{
    protected OrganizationLabel()
    {
    }

    public OrganizationLabel(
        string title,
        Guid organizationId,
        HexColor hexcolor = null,
        User createdBy = null)
        : base(title, hexcolor, createdBy)
    {
        OrganizationId = organizationId;
    }

    public Guid OrganizationId { get; protected set; }

    public virtual Organization Organization { get; protected set; }

    public virtual ICollection<CandidateCard> Cards { get; protected set; } = new List<CandidateCard>();

    public override void CouldBeUpdatedByOrFail(User user)
    {
        if (user.IsMyOrganization(OrganizationId) || user.Has(Role.Admin))
        {
            return;
        }

        throw new NoPermissionsException();
    }

    public bool CouldBeDeletedBy(User user)
    {
        return (user.IsMyOrganization(OrganizationId) &&
               Organization.ManagerId == user.Id) ||
               user.Has(Role.Admin);
    }
}