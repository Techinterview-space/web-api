using System;
using System.ComponentModel.DataAnnotations;
using Domain.Attributes;
using Domain.Entities.Enums;
using Domain.Entities.Users;
using Domain.Enums;
using Domain.Validation.Exceptions;

namespace Domain.Entities.Salaries;

public class UserSalary : HasDatesBase, IHasIdBase<Guid>
{
    protected UserSalary()
    {
    }

    public UserSalary(
        User user,
        double value,
        int quarter,
        int year,
        Currency currency,
        DeveloperGrade? grade,
        CompanyType company,
        Skill skillOrNull,
        WorkIndustry workIndustryOrNull,
        Profession professionOrNull,
        KazakhstanCity? city,
        bool useInStats)
    {
        Id = Guid.NewGuid();
        UserId = user.Id;
        User = user;

        Value = NonNegativeValue(value);
        Quarter = NonNegativeValue(quarter);
        Year = NonNegativeValue(year);

        Currency = currency;
        Grade = grade;
        Company = company;
        ProfessionEnum = professionOrNull?.IdAsEnum ?? UserProfessionEnum.Undefined; // TODO remove
        SkillId = skillOrNull?.Id;
        WorkIndustryId = workIndustryOrNull?.Id;
        ProfessionId = professionOrNull?.Id;

        City = city;
        UseInStats = useInStats;
    }

    public Guid Id { get; protected set; }

    public long? UserId { get; protected set; }

    public virtual User User { get; protected set; }

    public double Value { get; protected set; }

    [Range(1, 4)]
    public int Quarter { get; protected set; }

    [Range(2000, 3000)]
    public int Year { get; protected set; }

    [NotDefaultValue]
    public Currency Currency { get; protected set; }

    public DeveloperGrade? Grade { get; protected set; }

    [NotDefaultValue]
    public CompanyType Company { get; protected set; }

    public UserProfessionEnum ProfessionEnum { get; protected set; }

    public KazakhstanCity? City { get; protected set; }

    public int? Age { get; protected set; }

    public int? YearOfStartingWork { get; protected set; }

    public Gender? Gender { get; protected set; }

    public bool UseInStats { get; protected set; }

    public long? SkillId { get; protected set; }

    public virtual Skill Skill { get; protected set; }

    public long? WorkIndustryId { get; protected set; }

    public virtual WorkIndustry WorkIndustry { get; protected set; }

    public long? ProfessionId { get; protected set; }

    public virtual Profession Profession { get; protected set; }

    public void Update(
        DeveloperGrade grade,
        KazakhstanCity? city,
        CompanyType companyType,
        Skill skillOrNull,
        WorkIndustry workIndustryOrNull,
        Profession professionOrNull,
        int? age,
        int? yearOfStartingWork,
        Gender? gender)
    {
        Grade = grade;
        ProfessionEnum = professionOrNull?.IdAsEnum ?? UserProfessionEnum.Undefined;
        City = city;
        SkillId = skillOrNull?.Id;
        WorkIndustryId = workIndustryOrNull?.Id;
        ProfessionId = professionOrNull?.Id;

        Age = age;
        Company = companyType;
        YearOfStartingWork = yearOfStartingWork;
        Gender = gender;

        UpdatedAt = DateTime.UtcNow;
    }

    public void Approve()
    {
        if (UseInStats)
        {
            throw new BadRequestException("Salary record is already approved");
        }

        UseInStats = true;
        UpdatedAt = DateTime.UtcNow;
    }

    public void ExcludeFromStats()
    {
        if (!UseInStats)
        {
            throw new BadRequestException("Salary record is already excluded");
        }

        UseInStats = false;
        UpdatedAt = DateTime.UtcNow;
    }

    protected static double NonNegativeValue(double value)
    {
        if (value <= 0)
        {
            throw new BadRequestException("Value must be greater than 0");
        }

        return value;
    }

    protected static int NonNegativeValue(int value)
    {
        if (value <= 0)
        {
            throw new BadRequestException("Value must be greater than 0");
        }

        return value;
    }
}