using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Domain.Authentication.Abstract;
using Domain.Database;
using Domain.Entities.Salaries;
using Domain.Exceptions;
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
        request.IsValidOrFail();

        var currentUser = await _auth.CurrentUserAsync();
        var user = await _context.Users
            .FirstOrDefaultAsync(x => x.Id == currentUser.Id, cancellationToken);

        var hasRecordsForTheQuarter = await _context.Salaries
            .Where(x => x.UserId == user.Id)
            .Where(x => x.Quarter == request.Quarter)
            .Where(x => x.Year == request.Year)
            .AnyAsync(cancellationToken);

        if (hasRecordsForTheQuarter)
        {
            throw new BadRequestException("You already have a record for this quarter");
        }

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