﻿using System.Linq;
using System.Threading.Tasks;
using Infrastructure.Authentication.Contracts;
using Infrastructure.Database;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TechInterviewer.Features.Salaries.Models;
using TechInterviewer.Features.Users.Models;
using TechInterviewer.Setup.Attributes;

namespace TechInterviewer.Features.Accounts;

[ApiController]
[Route("api/account")]
[HasAnyRole]
public class AccountController : ControllerBase
{
    private readonly IAuthorization _auth;
    private readonly DatabaseContext _context;

    public AccountController(IAuthorization auth, DatabaseContext context)
    {
        _auth = auth;
        _context = context;
    }

    [HttpGet("me")]
    public async Task<UserDto> MeAsync()
    {
        var user = await _auth.CurrentUserOrFailAsync();
        var salaries = await _context.Salaries
            .Where(x => x.UserId == user.Id)
            .Select(x => new UserSalaryAdminDto
            {
                Id = x.Id,
                Value = x.Value,
                Quarter = x.Quarter,
                Year = x.Year,
                Currency = x.Currency,
                Grade = x.Grade,
                Company = x.Company,
                SkillId = x.SkillId,
                Age = x.Age,
                YearOfStartingWork = x.YearOfStartingWork,
                Gender = x.Gender,
                WorkIndustryId = x.WorkIndustryId,
                ProfessionId = x.ProfessionId,
                UpdatedAt = x.UpdatedAt,
                CreatedAt = x.CreatedAt,
            })
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();

        return new (user, salaries);
    }
}