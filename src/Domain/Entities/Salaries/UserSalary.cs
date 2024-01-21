﻿using System;
using System.ComponentModel.DataAnnotations;
using Domain.Attributes;
using Domain.Entities.Enums;
using Domain.Entities.Users;

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
        UserProfession profession,
        long? skillId)
    {
        Id = Guid.NewGuid();
        UserId = user.Id;
        User = user;
        Value = value;
        Quarter = quarter;
        Year = year;
        Currency = currency;
        Grade = grade;
        Company = company;
        Profession = profession;
        SkillId = skillId;
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
    public UserProfession Profession { get; protected set; }

    public long? SkillId { get; protected set; }

    public virtual Skill Skill { get; protected set; }

    public void Update(
        CompanyType company,
        DeveloperGrade? grade)
    {
        Company = company;
        Grade = grade;
    }
}