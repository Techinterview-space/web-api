using System.Collections.Generic;
using Domain.Entities.Salaries;
using Domain.Services.Salaries;

namespace TechInterviewer.Controllers.Salaries.Charts;

public record SalariesByProfession
{
    public SalariesByProfession(
        UserProfession profession,
        List<UserSalaryDto> salaries)
    {
        Profession = profession;
        Salaries = salaries;
    }

    public UserProfession Profession { get; }

    public List<UserSalaryDto> Salaries { get; }
}