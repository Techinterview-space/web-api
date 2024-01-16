using System;
using Domain.Services.Salaries;

namespace TechInterviewer.Controllers.Salaries;

public record UserSalaryAdminDto : UserSalaryDto
{
    public Guid Id { get; init; }
}