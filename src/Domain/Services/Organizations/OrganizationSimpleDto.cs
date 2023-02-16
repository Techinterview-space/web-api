using System;
using Domain.Entities.Organizations;
using MG.Utils.Abstract.Entities;

namespace Domain.Services.Organizations;

public record OrganizationSimpleDto : IHasIdBase<Guid>
{
    public OrganizationSimpleDto()
    {
    }

    public OrganizationSimpleDto(
        Organization organization)
    {
        Id = organization.Id;
        Name = organization.Name;
        ManagerId = organization.ManagerId;
    }

    public Guid Id { get; init; }

    public string Name { get; init; }

    public long? ManagerId { get; init; }

    public static OrganizationSimpleDto CreateFromEntityOrNull(Organization organizationOrNull)
    {
        return organizationOrNull is not null
            ? new OrganizationSimpleDto(organizationOrNull)
            : null;
    }
}