using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Domain.Authentication.Abstract;
using Domain.Database;
using Domain.Entities.Salaries;
using Domain.Entities.Users;
using Domain.Enums;
using Domain.Exceptions;
using Domain.Services;
using Domain.Services.Salaries;
using Domain.ValueObjects.Dates;
using Domain.ValueObjects.Pagination;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SendGrid.Helpers.Errors.Model;
using TechInterviewer.Controllers.Salaries.AdminChart;
using TechInterviewer.Controllers.Salaries.Charts;
using TechInterviewer.Controllers.Salaries.CreateSalaryRecord;
using TechInterviewer.Controllers.Salaries.GetAllSalaries;
using TechInterviewer.Setup.Attributes;
using BadRequestException = Domain.Exceptions.BadRequestException;

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
        return await GetAllSalariesQuery(request, true)
            .AsPaginatedAsync(request, cancellationToken);
    }

    [HttpGet("not-in-stats")]
    [HasAnyRole(Role.Admin)]
    public async Task<Pageable<UserSalaryAdminDto>> AllNotShownInStatsAsync(
        [FromQuery] GetAllSalariesRequest request,
        CancellationToken cancellationToken)
    {
        return await GetAllSalariesQuery(request, false)
            .AsPaginatedAsync(request, cancellationToken);
    }

    [HttpGet("salaries-adding-trend-chart")]
    [HasAnyRole(Role.Admin)]
    public async Task<AdminChartResponse> AdminChart(
        CancellationToken cancellationToken)
    {
        var currentDay = DateTime.UtcNow;
        var fifteenDaysAgo = currentDay.AddDays(-20);
        var salaries = await _context.Salaries
            .Where(x => x.CreatedAt >= fifteenDaysAgo)
            .Select(x => new
            {
                x.Id,
                x.UserId,
                x.CreatedAt
            })
            .AsNoTracking()
            .OrderBy(x => x.CreatedAt)
            .ToListAsync(cancellationToken);

        var daysSplitter = new DateTimeRoundedRangeSplitter(fifteenDaysAgo, currentDay, 360);
        var response = new AdminChartResponse
        {
            SalariesPerUser = (double)salaries.Count / salaries.GroupBy(x => x.UserId).Count()
        };

        foreach (var (start, end) in daysSplitter.ToList())
        {
            var count = salaries.Count(x =>
                x.CreatedAt >= start &&
                (x.CreatedAt < end || x.CreatedAt == end));

            response.Items.Add(new AdminChartResponse.AdminChartItem(count, start));
            response.Labels.Add(start.ToString(AdminChartResponse.DateTimeFormat));
        }

        return response;
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

        var yearAgoGap = DateTimeOffset.Now.AddYears(-1);
        var query = _context.Salaries
            .Where(x => x.UseInStats)
            .Where(x => x.Profession != UserProfession.HrNonIt)
            .Where(x => x.CreatedAt >= yearAgoGap)
            .When(request.Grade.HasValue, x => x.Grade == request.Grade.Value)
            .When(request.ProfessionsToInclude.Any(), x => request.ProfessionsToInclude.Contains(x.Profession))
            .When(request.ProfessionsToExclude.Any(), x => !request.ProfessionsToExclude.Contains(x.Profession))
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
            .AsNoTracking();

        if (!userSalariesForLastYear.Any())
        {
            return SalariesChartResponse.RequireOwnSalary(
                await query.CountAsync(cancellationToken));
        }

        var salaries = await query.ToListAsync(cancellationToken);

        return new SalariesChartResponse(
            salaries,
            new UserSalaryAdminDto(userSalariesForLastYear.First()),
            yearAgoGap,
            DateTimeOffset.Now,
            salaries.Count);
    }

    [HttpPost("")]
    public async Task<CreateOrEditSalaryRecordResponse> Create(
        [FromBody] CreateOrEditSalaryRecordRequest request,
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
            return CreateOrEditSalaryRecordResponse.Failure("You already have a record for this quarter");
        }

        Skill skill = null;
        if (request.SkillId is > 0)
        {
            skill = await _context.Skills
                .FirstOrDefaultAsync(x => x.Id == request.SkillId.Value, cancellationToken);
        }

        var shouldShowInStats = await new UserSalaryShowInStatsDecisionMaker(
            _context,
            request.Value,
            request.Grade,
            request.Company,
            request.Profession)
            .DecideAsync(cancellationToken);

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
                skill?.Id,
                shouldShowInStats),
            cancellationToken);

        return CreateOrEditSalaryRecordResponse.Success(
            new UserSalaryDto(salary));
    }

    [HttpPost("{id:guid}")]
    public async Task<CreateOrEditSalaryRecordResponse> Update(
        [FromRoute] Guid id,
        [FromBody] EditSalaryRequest request,
        CancellationToken cancellationToken)
    {
        request.IsValidOrFail();
        var salary = await _context.Salaries
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
            ?? throw new ResourceNotFoundException("Salary record not found");

        var currentUser = await _auth.CurrentUserAsync();

        if (!currentUser.Has(Role.Admin) &&
            salary.UserId != currentUser.Id)
        {
            throw new ForbiddenException("You can only edit your own salary records");
        }

        salary.Update(request.Grade);

        await _context.SaveChangesAsync(cancellationToken);
        return CreateOrEditSalaryRecordResponse.Success(new UserSalaryDto(salary));
    }

    [HttpPost("{id:guid}/approve")]
    [HasAnyRole(Role.Admin)]
    public async Task<IActionResult> Approve(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        var salary = await _context.Salaries
                         .FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
                     ?? throw new ResourceNotFoundException("Salary record not found");

        salary.Approve();
        await _context.SaveChangesAsync(cancellationToken);

        return Ok();
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

    private IQueryable<UserSalaryAdminDto> GetAllSalariesQuery(
        GetAllSalariesRequest request,
        bool? showInStats)
    {
        return _context.Salaries
            .When(request.CompanyType.HasValue, x => x.Company == request.CompanyType.Value)
            .When(request.Grade.HasValue, x => x.Grade == request.Grade.Value)
            .When(request.Profession.HasValue, x => x.Profession == request.Profession.Value)
            .When(showInStats.HasValue, x => x.UseInStats == showInStats.Value)
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
            .OrderByDescending(x => x.CreatedAt);
    }
}