using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Domain.Entities.Users;
using Infrastructure.Salaries;

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
        Salaries = user.Salaries?
            .Select(x => new UserSalaryDto(x))
            .ToList();
    }

    public List<UserSalaryDto> Salaries { get; init; }

    public int SalariesCount => Salaries?.Count ?? 0;

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
            Salaries = user.Salaries != null
                ? user.Salaries
                    .Select(x => new UserSalaryDto
                    {
                        Age = x.Age,
                        City = x.City,
                        Company = x.Company,
                        CreatedAt = x.CreatedAt,
                        Currency = x.Currency,
                        Gender = x.Gender,
                        Grade = x.Grade,
                        ProfessionId = x.ProfessionId,
                        Quarter = x.Quarter,
                        SkillId = x.SkillId,
                        SourceType = x.SourceType,
                        UpdatedAt = x.UpdatedAt,
                        Value = x.Value,
                        WorkIndustryId = x.WorkIndustryId,
                        Year = x.Year,
                        YearOfStartingWork = x.YearOfStartingWork,
                    })
                    .ToList()
                : null,
            IsMfaEnabled = user.TotpSecret != null,
            CreatedAt = user.CreatedAt,
            DeletedAt = user.DeletedAt,
        };
}