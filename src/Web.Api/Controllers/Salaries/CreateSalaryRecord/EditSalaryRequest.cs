using Domain.Entities.Enums;
using Domain.Exceptions;

namespace TechInterviewer.Controllers.Salaries.CreateSalaryRecord;

public record EditSalaryRequest
{
    public DeveloperGrade Grade { get; init; }

    public long? SkillId { get; init; }

    public virtual void IsValidOrFail()
    {
        if (Grade == DeveloperGrade.Unknown)
        {
            throw new BadRequestException("Grade must be valid");
        }
    }
}