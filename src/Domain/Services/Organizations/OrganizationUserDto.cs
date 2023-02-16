using System;
using Domain.Entities.Organizations;
using Domain.Services.Users;

namespace Domain.Services.Organizations;

public record OrganizationUserDto
{
    public OrganizationUserDto()
    {
    }

    public OrganizationUserDto(
        OrganizationUser organizationUser,
        bool setUser = false,
        bool setOrganization = false)
    {
        Id = organizationUser.Id;
        UserId = organizationUser.UserId;
        User = setUser && organizationUser.User != null
            ? new UserDto(organizationUser.User)
            : null;

        Organization = setOrganization && organizationUser.Organization != null
            ? new OrganizationDto(organizationUser.Organization)
            : null;

        OrganizationId = organizationUser.OrganizationId;
        DeletedAt = organizationUser.DeletedAt;
        CreatedAt = organizationUser.CreatedAt;
        UpdatedAt = organizationUser.UpdatedAt;
    }

    public long Id { get; init; }

    public long UserId { get; init; }

    public UserDto User { get; init; }

    public Guid OrganizationId { get; init; }

    public OrganizationSimpleDto Organization { get; init; }

    public DateTimeOffset? DeletedAt { get; init; }

    public DateTimeOffset CreatedAt { get; init; }

    public DateTimeOffset UpdatedAt { get; init; }

    public bool Active => DeletedAt == null;
}