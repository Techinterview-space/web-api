using System;
using Domain.Entities.Salaries;
using Domain.Services.Salaries;

namespace TechInterviewer.Controllers.Salaries;

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