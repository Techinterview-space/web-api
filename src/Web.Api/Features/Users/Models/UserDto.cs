using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Domain.Entities;
using Domain.Entities.Users;
using Domain.Enums;
using Domain.Validation;

namespace Web.Api.Features.Users.Models;

public record UserDto : IHasId
{
    public UserDto()
    {
    }

    public UserDto(
        User user)
    {
        user.ThrowIfNull(nameof(user));
        Id = user.Id;
        Email = user.Email;
        FirstName = user.FirstName;
        LastName = user.LastName;
        Roles = user.UserRoles?.Select(x => x.RoleId).ToList() ?? new List<Role>(0);
        EmailConfirmed = user.EmailConfirmed;
        IsMfaEnabled = user.IsMfaEnabled();
        IsGoogleAuth = user.IsGoogleAuth();
        IsGithubAuth = user.IsGithubAuth();
        IsAuth0Auth = user.IsAuth0Auth();
        PictureProfile = user.ProfilePicture;

        CreatedAt = user.CreatedAt;
        DeletedAt = user.DeletedAt;
    }

    public long Id { get; init; }

    public string Email { get; init; }

    public bool EmailConfirmed { get; init; }

    public string FirstName { get; init; }

    public string LastName { get; init; }

    public string PictureProfile { get; init; }

    public string Fullname => $"{FirstName} {LastName}";

    public bool IsMfaEnabled { get; init; }

    public bool IsGoogleAuth { get; init; }

    public bool IsGithubAuth { get; init; }

    public bool IsAuth0Auth { get; init; }

    public List<Role> Roles { get; init; }

    public DateTimeOffset CreatedAt { get; init; }

    public DateTimeOffset? DeletedAt { get; init; }

    public static UserDto CreateFromEntityOrNull(
        User user)
    {
        return user is not null ? new UserDto(user) : null;
    }

    public static readonly Expression<Func<User, UserDto>> Transformation =
        user => new UserDto
        {
            Id = user.Id,
            Email = user.Email,
            EmailConfirmed = user.EmailConfirmed,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Roles = user.UserRoles != null
                ? user.UserRoles
                    .Select(x => x.RoleId)
                    .ToList()
                : null,
            IsMfaEnabled = user.TotpSecret != null,
            PictureProfile = user.ProfilePicture,
            CreatedAt = user.CreatedAt,
            DeletedAt = user.DeletedAt,
        };
}