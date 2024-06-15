using System.Collections.Generic;
using System.Linq;
using Domain.Attributes;
using Domain.Enums;

namespace Web.Api.Features.Users.Models;

public record UserUpdateRolesRequest
{
    [NotDefaultValue]
    public long Id { get; init; }

    public IReadOnlyCollection<Role> Roles { get; init; }

    public bool HasRoles() => Roles != null && Roles.Any();
}