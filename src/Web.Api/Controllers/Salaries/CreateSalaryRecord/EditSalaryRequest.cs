using Domain.Entities.Enums;
using Domain.Entities.Salaries;
using Domain.Enums;
using Domain.Exceptions;

namespace TechInterviewer.Controllers.Salaries.CreateSalaryRecord;

public record EditSalaryRequest
{
    public DeveloperGrade Grade { get; init; }

    public UserProfession Profession { get; init; }

    public KazakhstanCity? City { get; init; }

    public long? SkillId { get; init; }

    public long? WorkIndustryId { get; init; }

    public CompanyType Company { get; init; }

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

        if (City == KazakhstanCity.Undefined)
        {
            throw new BadRequestException("City must be valid");
        }

        if (Company == default)
        {
            throw new BadRequestException("Company must be specified");
        }
    }
}