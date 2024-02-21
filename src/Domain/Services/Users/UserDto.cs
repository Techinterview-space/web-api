using System;
using System.Collections.Generic;
using System.Linq;
using Domain.Entities.Users;
using Domain.Enums;
using Domain.Services.Organizations;
using Domain.Validation;

namespace Domain.Services.Users;

public record UserDto
{
    public UserDto(User user)
    {
        user.ThrowIfNull(nameof(user));
        Id = user.Id;
        Email = user.Email;
        FirstName = user.FirstName;
        LastName = user.LastName;
        Roles = user.UserRoles?.Select(x => x.RoleId).ToArray() ?? Array.Empty<Role>();
        EmailConfirmed = user.EmailConfirmed;
        CreatedAt = user.CreatedAt;
        DeletedAt = user.DeletedAt;
    }

    public long Id { get; }

    public string Email { get; }

    public bool EmailConfirmed { get; }

    public string FirstName { get; }

    public string LastName { get; }

    public string Fullname => $"{FirstName} {LastName}";

    public IReadOnlyCollection<Role> Roles { get; }

    public DateTimeOffset CreatedAt { get; }

    public DateTimeOffset? DeletedAt { get; }

    public static UserDto CreateFromEntityOrNull(User user)
    {
        return user is not null ? new UserDto(user) : null;
    }
}