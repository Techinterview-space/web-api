using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Domain.Authentication.Abstract;
using Domain.Database;
using Domain.Entities.Salaries;
using Domain.Services.Salaries;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TechInterviewer.Setup.Attributes;

namespace TechInterviewer.Controllers.Salaries;

[ApiController]
[Route("api/salaries")]
[HasAnyRole]
public class SalariesController : ControllerBase
{
    private readonly IAuthorization _auth;
    private readonly DatabaseContext _context;

    public SalariesController(
        IAuthorization auth,
        DatabaseContext context)
    {
        _auth = auth;
        _context = context;
    }

    [HttpGet("all")]
    public async Task<List<UserSalaryDto>> AllAsync(
        CancellationToken cancellationToken)
    {
        var yearAgoGap = DateTimeOffset.Now.AddYears(-1);
        return await _context.Salaries
            .Where(x => x.CreatedAt >= yearAgoGap)
            .Select(x => new UserSalaryDto
            {
                Value = x.Value,
                Quarter = x.Quarter,
                Year = x.Year,
                Currency = x.Currency,
                Company = x.Company,
                Grage = x.Grage,
                CreatedAt = x.CreatedAt
            })
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    [HttpPost("")]
    public async Task<UserSalaryDto> Create(
        [FromBody] CreateSalaryRecordRequest request,
        CancellationToken cancellationToken)
    {
        var currentUser = await _auth.CurrentUserAsync();
        var user = await _context.Users
            .FirstOrDefaultAsync(x => x.Id == currentUser.Id, cancellationToken);

        // TODO mgorbatyuk:
        // 1. Add validation for duplicated salary for same quarter and year
        // 2. Add validation for future dates
        var salary = await _context.SaveAsync(
            new UserSalary(
                user,
                request.Value,
                request.Quarter,
                request.Year,
                request.Currency,
                request.Grage,
                request.Company),
            cancellationToken);

        return new UserSalaryDto(salary);
    }
}