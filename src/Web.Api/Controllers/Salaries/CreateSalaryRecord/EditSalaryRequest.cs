using Domain.Entities.Enums;
using Domain.Entities.Salaries;
using Domain.Exceptions;

namespace TechInterviewer.Controllers.Salaries.CreateSalaryRecord;

public record EditSalaryRequest
{
    public DeveloperGrade Grade { get; init; }

    public UserProfession Profession { get; init; }

    public long? SkillId { get; init; }

    public virtual void IsValidOrFail()
    {
        if (Grade == DeveloperGrade.Unknown)
        {
            throw new BadRequestException("Grade must be valid");
        }

        if (Profession == UserProfession.Undefined)
        {
            throw new BadRequestException("Profession must be valid");
        }
    }
}