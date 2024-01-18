﻿using Domain.Entities.Enums;
using Domain.Entities.Salaries;
using Domain.ValueObjects.Pagination;
using Microsoft.AspNetCore.Mvc;

namespace TechInterviewer.Controllers.Salaries.GetAllSalaries;

public record GetAllSalariesRequest : PageModel
{
    [FromQuery(Name = "company")]
    public CompanyType? CompanyType { get; init; }

    [FromQuery(Name = "grade")]
    public DeveloperGrade? Grade { get; init; }

    [FromQuery(Name = "profession")]
    public UserProfession? Profession { get; init; }
}