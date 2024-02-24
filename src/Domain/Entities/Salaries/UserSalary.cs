using System;
using System.ComponentModel.DataAnnotations;
using Domain.Attributes;
using Domain.Entities.Enums;
using Domain.Entities.Users;
using Domain.Enums;
using Domain.Exceptions;

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
        UserProfessionEnum profession,
        Skill skillOrNull,
        WorkIndustry workIndustryOrNull,
        UserProfession userProfessionOrNull,
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
        Profession = profession;
        SkillId = skillOrNull?.Id;
        WorkIndustryId = workIndustryOrNull?.Id;
        UserProfessionId = userProfessionOrNull?.Id;

        City = city;
        UseInStats = useInStats;
    }

    public Guid Id { get; }

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

    [NotDefaultValue]
    public UserProfessionEnum Profession { get; protected set; }

    public KazakhstanCity? City { get; protected set; }

    public bool UseInStats { get; protected set; }

    public long? SkillId { get; protected set; }

    public virtual Skill Skill { get; protected set; }

    public long? WorkIndustryId { get; protected set; }

    public virtual WorkIndustry WorkIndustry { get; protected set; }

    public long? UserProfessionId { get; protected set; }

    public virtual UserProfession UserProfession { get; protected set; }

    public void Update(
        DeveloperGrade grade,
        UserProfessionEnum profession,
        KazakhstanCity? city,
        CompanyType companyType,
        Skill skillOrNull,
        WorkIndustry workIndustryOrNull,
        UserProfession userProfessionOrNull)
    {
        Grade = grade;
        Profession = profession;
        City = city;
        SkillId = skillOrNull?.Id;
        WorkIndustryId = workIndustryOrNull?.Id;
        UserProfessionId = userProfessionOrNull?.Id;

        Company = companyType;

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

    private static double NonNegativeValue(double value)
    {
        if (value <= 0)
        {
            throw new BadRequestException("Value must be greater than 0");
        }

        return value;
    }

    private static int NonNegativeValue(int value)
    {
        if (value <= 0)
        {
            throw new BadRequestException("Value must be greater than 0");
        }

        return value;
    }
}