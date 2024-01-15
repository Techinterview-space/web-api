using Domain.Services.Salaries;

namespace TechInterviewer.Controllers.Salaries.GetAllSalaries;

public record UserSalaryAdminDto : UserSalaryDto
{
    public long? UserId { get; init; }

    public string UserEmail { get; init; }
}