using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Domain.Authentication.Abstract;
using Domain.Database;
using Domain.Entities.Salaries;
using Domain.Enums;
using Domain.Exceptions;
using Domain.Services.Salaries;
using Domain.ValueObjects.Dates;
using Domain.ValueObjects.Pagination;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TechInterviewer.Controllers.Salaries.Charts;
using TechInterviewer.Controllers.Salaries.CreateSalaryRecord;
using TechInterviewer.Controllers.Salaries.GetAllSalaries;
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
    [HasAnyRole(Role.Admin)]
    public async Task<Pageable<UserSalaryAdminDto>> AllAsync(
        [FromQuery] GetAllSalariesRequest request,
        CancellationToken cancellationToken)
    {
        return await _context.Salaries
            .Select(x => new UserSalaryAdminDto
            {
                Id = x.Id,
                Value = x.Value,
                Quarter = x.Quarter,
                Year = x.Year,
                Currency = x.Currency,
                Company = x.Company,
                Grade = x.Grade,
                Profession = x.Profession,
                CreatedAt = x.CreatedAt
            })
            .AsNoTracking()
            .AsPaginatedAsync(request, cancellationToken);
    }

    [HttpGet("chart")]
    public async Task<SalariesChartResponse> ChartAsync(
        CancellationToken cancellationToken)
    {
        var currentUser = await _auth.CurrentUserAsync();
        var user = await _context.Users
            .FirstOrDefaultAsync(x => x.Id == currentUser.Id, cancellationToken);

        var currentQuarter = DateQuarter.Current;
        var hasRecordsForAnyQuarterForLastYear = await _context.Salaries
            .Where(x => x.UserId == user.Id)
            .Where(x => x.Year == currentQuarter.Year || x.Year == currentQuarter.Year - 1)
            .AnyAsync(cancellationToken);

        if (!hasRecordsForAnyQuarterForLastYear)
        {
            return SalariesChartResponse.RequireOwnSalary();
        }

        var yearAgoGap = DateTimeOffset.Now.AddYears(-1);
        var salaries = await _context.Salaries
            .Where(x => x.CreatedAt >= yearAgoGap)
            .Select(x => new UserSalaryDto
            {
                Value = x.Value,
                Quarter = x.Quarter,
                Year = x.Year,
                Currency = x.Currency,
                Company = x.Company,
                Grade = x.Grade,
                Profession = x.Profession,
                CreatedAt = x.CreatedAt
            })
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        return new SalariesChartResponse(
            salaries,
            yearAgoGap,
            DateTimeOffset.Now);
    }

    [HttpPost("")]
    public async Task<CreateSalaryRecordResponse> Create(
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
            return CreateSalaryRecordResponse.Failure("You already have a record for this quarter");
        }

        var salary = await _context.SaveAsync(
            new UserSalary(
                user,
                request.Value,
                request.Quarter,
                request.Year,
                request.Currency,
                request.Grade,
                request.Company,
                request.Profession),
            cancellationToken);

        return CreateSalaryRecordResponse.Success(
            new UserSalaryDto(salary));
    }

    [HttpDelete("{id:guid}")]
    [HasAnyRole(Role.Admin)]
    public async Task<IActionResult> Delete(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        var salary = await _context.Salaries
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
            ?? throw new ResourceNotFoundException("Salary record not found");

        _context.Salaries.Remove(salary);
        await _context.SaveChangesAsync(cancellationToken);

        return Ok();
    }
}