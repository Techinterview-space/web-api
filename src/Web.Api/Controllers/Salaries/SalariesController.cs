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
using TechInterviewer.Controllers.Salaries.UpdateSalary;
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
            .When(request.CompanyType.HasValue, x => x.Company == request.CompanyType.Value)
            .When(request.Grade.HasValue, x => x.Grade == request.Grade.Value)
            .When(request.Profession.HasValue, x => x.Profession == request.Profession.Value)
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
            .OrderByDescending(x => x.CreatedAt)
            .AsPaginatedAsync(request, cancellationToken);
    }

    [HttpGet("chart")]
    public async Task<SalariesChartResponse> ChartAsync(
        [FromQuery] SalariesChartQueryParams request,
        CancellationToken cancellationToken)
    {
        var currentUser = await _auth.CurrentUserAsync();
        var user = await _context.Users
            .FirstOrDefaultAsync(x => x.Id == currentUser.Id, cancellationToken);

        var currentQuarter = DateQuarter.Current;
        var userSalariesForLastYear = await _context.Salaries
            .Where(x => x.UserId == user.Id)
            .Where(x => x.Year == currentQuarter.Year || x.Year == currentQuarter.Year - 1)
            .AsNoTracking()
            .OrderByDescending(x => x.Year)
            .ThenByDescending(x => x.Quarter)
            .ToListAsync(cancellationToken);

        if (!userSalariesForLastYear.Any())
        {
            return SalariesChartResponse.RequireOwnSalary();
        }

        var yearAgoGap = DateTimeOffset.Now.AddYears(-1);
        var salaries = await _context.Salaries
            .Where(x => x.CreatedAt >= yearAgoGap)
            .When(request.Grade.HasValue, x => x.Grade == request.Grade.Value)
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
            .OrderBy(x => x.Value)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        return new SalariesChartResponse(
            salaries,
            new UserSalaryDto(userSalariesForLastYear.First()),
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

        Skill skill = null;
        if (request.SkillId is > 0)
        {
            skill = await _context.Skills
                .FirstOrDefaultAsync(x => x.Id == request.SkillId.Value, cancellationToken);
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
                request.Profession,
                skill?.Id),
            cancellationToken);

        return CreateSalaryRecordResponse.Success(
            new UserSalaryDto(salary));
    }

    [HttpPost("{id:guid}")]
    [HasAnyRole(Role.Admin)]
    public async Task<IActionResult> Update(
        [FromRoute] Guid id,
        [FromBody] UpdateSalaryRequest request,
        CancellationToken cancellationToken)
    {
        request.IsValidOrFail();
        var salary = await _context.Salaries
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
            ?? throw new ResourceNotFoundException("Salary record not found");

        salary.Update(
            request.Company,
            request.Grade);

        await _context.SaveChangesAsync(cancellationToken);
        return Ok(new UserSalaryDto(salary));
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