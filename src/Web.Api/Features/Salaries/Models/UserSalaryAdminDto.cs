using System;
using Domain.Entities.Salaries;
using Infrastructure.Salaries;

namespace Web.Api.Features.Salaries.Models;

public record UserSalaryAdminDto : UserSalaryDto
{
    public Guid Id { get; init; }

    public UserSalaryAdminDto()
    {
    }

    public UserSalaryAdminDto(
        UserSalary salary)
        : base(salary)
    {
        Id = salary.Id;
    }
}