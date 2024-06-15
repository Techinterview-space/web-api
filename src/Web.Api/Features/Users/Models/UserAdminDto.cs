using System;
using System.Linq;
using System.Linq.Expressions;
using Domain.Entities.Users;

namespace Web.Api.Features.Users.Models;

public record UserAdminDto : UserDto
{
    public UserAdminDto()
    {
    }

    public UserAdminDto(
        User user)
        : base(user)
    {
        SalariesCount = user.Salaries?.Count ?? 0;
    }

    public int SalariesCount { get; init; }

    public static readonly Expression<Func<User, UserAdminDto>> Transformation =
        user => new UserAdminDto
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
            SalariesCount = user.Salaries != null
                ? user.Salaries.Count
                : 0,
            CreatedAt = user.CreatedAt,
            DeletedAt = user.DeletedAt,
        };
}