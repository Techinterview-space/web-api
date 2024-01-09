using System;
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
        DeveloperGrade? grage,
        CompanyType company)
    {
        Id = Guid.NewGuid();
        UserId = user.Id;
        User = user;
        Value = value;
        Quarter = quarter;
        Year = year;
        Currency = currency;
        Grage = grage;
        Company = company;
    }

    public Guid Id { get; }

    public long? UserId { get; protected set; }

    public virtual User User { get; protected set; }

    public double Value { get; protected set; }

    public int Quarter { get; protected set; }

    public int Year { get; protected set; }

    public Currency Currency { get; protected set; }

    public DeveloperGrade? Grage { get; protected set; }

    public CompanyType Company { get; protected set; }
}