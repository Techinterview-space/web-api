using System.Linq;
using System.Threading.Tasks;
using Domain.Authentication.Abstract;
using Domain.Database;
using Domain.Services.Salaries;
using Domain.Services.Users;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TechInterviewer.Controllers.Salaries;
using TechInterviewer.Controllers.Users;
using TechInterviewer.Setup.Attributes;

namespace TechInterviewer.Controllers;

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