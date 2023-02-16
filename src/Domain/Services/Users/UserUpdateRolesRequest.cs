using System.Collections.Generic;
using System.Linq;
using Domain.Enums;
using MG.Utils.Attributes;

namespace Domain.Services.Users;

public record UserUpdateRolesRequest
{
    [NotDefaultValue]
    public long Id { get; init; }

    public IReadOnlyCollection<Role> Roles { get; init; }

    public bool HasRoles() => Roles != null && Roles.Any();
}