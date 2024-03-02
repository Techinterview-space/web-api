using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Domain.Authentication.Abstract;
using Domain.Database;
using Domain.Entities.Salaries;
using Domain.Entities.Users;
using Domain.Enums;
using Domain.Exceptions;
using Domain.Extensions;
using Domain.Services;
using Domain.Services.Global;
using Domain.Services.Salaries;
using Domain.ValueObjects.Dates;
using Domain.ValueObjects.Pagination;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SendGrid.Helpers.Errors.Model;
using TechInterviewer.Controllers.Labels;
using TechInterviewer.Controllers.Salaries.AdminChart;
using TechInterviewer.Controllers.Salaries.Charts;
using TechInterviewer.Controllers.Salaries.CreateSalaryRecord;
using TechInterviewer.Controllers.Salaries.GetAllSalaries;
using TechInterviewer.Features.Charts;
using TechInterviewer.Setup.Attributes;

namespace TechInterviewer.Controllers.Salaries;

[ApiController]
[Route("api/salaries")]
public class SalariesController : ControllerBase
{
    private readonly IAuthorization _auth;
    private readonly DatabaseContext _context;
    private readonly IGlobal _global;

    public SalariesController(
        IAuthorization auth,
        DatabaseContext context,
        IGlobal global)
    {
        _auth = auth;
        _context = context;
        _global = global;
    }

    [HttpGet("select-box-items")]
    public async Task<SelectBoxItemsResponse> GetSelectBoxItems(
        CancellationToken cancellationToken)
    {
        return new SelectBoxItemsResponse
        {
            Skills = await _context.Skills
                .Select(x => new LabelEntityDto
                {
                    Id = x.Id,
                    Title = x.Title,
                    HexColor = x.HexColor,
                })
                .AsNoTracking()
                .ToListAsync(cancellationToken),
            Industries = await _context.WorkIndustries
                .Select(x => new LabelEntityDto
                {
                    Id = x.Id,
                    Title = x.Title,
                    HexColor = x.HexColor,
                })
                .AsNoTracking()
                .ToListAsync(cancellationToken),
            Professions = await _context.Professions
                .Select(x => new LabelEntityDto
                {
                    Id = x.Id,
                    Title = x.Title,
                    HexColor = x.HexColor,
                })
                .AsNoTracking()
                .ToListAsync(cancellationToken),
        };
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

        var usersCount = await _context.Users
            .AsNoTracking()
            .CountAsync(cancellationToken);

        var salaries = await _context.Salaries
            .Select(x => new
            {
                x.Id,
                x.UserId,
                x.CreatedAt
            })
            .AsNoTracking()
            .OrderBy(x => x.CreatedAt)
            .ToListAsync(cancellationToken);

        var usersWhoLeftSalaries = salaries.GroupBy(x => x.UserId).Count();

        var response = new AdminChartResponse
        {
            SalariesPerUser = (double)salaries.Count / usersWhoLeftSalaries,
            UsersWhoLeftSalary = usersWhoLeftSalaries,
            AllUsersCount = usersCount,
        };

        var salariesFifteenDaysAgoAdded = salaries
            .Where(x => x.CreatedAt >= fifteenDaysAgo)
            .ToList();

        var daysSplitter = new DateTimeRoundedRangeSplitter(fifteenDaysAgo, currentDay, 1440);

        foreach (var (start, end) in daysSplitter.ToList())
        {
            var count = salariesFifteenDaysAgoAdded.Count(x =>
                x.CreatedAt >= start &&
                (x.CreatedAt < end || x.CreatedAt == end));

            response.Items.Add(new AdminChartResponse.AdminChartItem(count, start));
            response.Labels.Add(start.ToString(AdminChartResponse.DateTimeFormat));
        }

        return response;
    }

    [HttpGet("chart")]
    public Task<SalariesChartResponse> ChartAsync(
        [FromQuery] SalariesChartQueryParams request,
        CancellationToken cancellationToken)
    {
        return new UserChartHandler(_auth, _context).Handle(request, cancellationToken);
    }

    [HttpGet("chart-share")]
    public async Task<IActionResult> ChartShareAsync(
        [FromQuery] SalariesChartQueryParams request,
        CancellationToken cancellationToken)
    {
        var result = await new UserChartHandler(_auth, _context).Handle(request, cancellationToken);
        var contentResultProvider = new ChartShareRedirectPageContentResultHandler(
            result,
            request,
            _context,
            _global);

        return await contentResultProvider.CreateAsync(
            Response,
            Request.QueryString.Value,
            cancellationToken);
    }

    [HttpPost("")]
    [HasAnyRole]
    public async Task<CreateOrEditSalaryRecordResponse> Create(
        [FromBody] CreateOrEditSalaryRecordRequest request,
        CancellationToken cancellationToken)
    {
        request.IsValidOrFail();

        var currentUser = await _auth.CurrentUserOrNullAsync();
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

        WorkIndustry workIndustry = null;
        if (request.WorkIndustryId is > 0)
        {
            workIndustry = await _context.WorkIndustries
                .FirstOrDefaultAsync(x => x.Id == request.WorkIndustryId.Value, cancellationToken);
        }

        Profession profession = null;
        if (request.ProfessionId is > 0)
        {
            var professionEnumAsLong = (long)request.ProfessionId;
            profession = await _context.Professions
                .FirstOrDefaultAsync(x => x.Id == request.ProfessionId || x.Id == professionEnumAsLong, cancellationToken);
        }

        var shouldShowInStats = await new UserSalaryShowInStatsDecisionMaker(
            _context,
            _auth.CurrentUser,
            request.Value,
            request.Grade,
            request.Company,
            profession)
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
                skill,
                workIndustry,
                profession,
                request.City,
                shouldShowInStats),
            cancellationToken);

        return CreateOrEditSalaryRecordResponse.Success(
            new UserSalaryDto(salary));
    }

    [HttpPost("{id:guid}")]
    [HasAnyRole]
    public async Task<CreateOrEditSalaryRecordResponse> Update(
        [FromRoute] Guid id,
        [FromBody] EditSalaryRequest request,
        CancellationToken cancellationToken)
    {
        request.IsValidOrFail();
        var salary = await _context.Salaries
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
            ?? throw new ResourceNotFoundException("Salary record not found");

        var currentUser = await _auth.CurrentUserOrNullAsync();

        if (!currentUser.Has(Role.Admin) &&
            salary.UserId != currentUser.Id)
        {
            throw new ForbiddenException("You can only edit your own salary records");
        }

        Skill skill = null;
        if (request.SkillId is > 0)
        {
            skill = await _context.Skills
                .FirstOrDefaultAsync(x => x.Id == request.SkillId.Value, cancellationToken);
        }

        WorkIndustry workIndustry = null;
        if (request.WorkIndustryId is > 0)
        {
            workIndustry = await _context.WorkIndustries
                .FirstOrDefaultAsync(x => x.Id == request.WorkIndustryId.Value, cancellationToken);
        }

        Profession profession = null;
        if (request.ProfessionId is > 0)
        {
            profession = await _context.Professions
                .FirstOrDefaultAsync(x => x.Id == request.ProfessionId, cancellationToken);
        }

        salary.Update(
            grade: request.Grade,
            city: request.City,
            companyType: request.Company,
            skillOrNull: skill,
            workIndustryOrNull: workIndustry,
            professionOrNull: profession,
            age: request.Age,
            yearOfStartingWork: request.YearOfStartingWork,
            gender: request.Gender);

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

    [HttpPost("{id:guid}/exclude-from-stats")]
    [HasAnyRole(Role.Admin)]
    public async Task<IActionResult> ExcludeFromStats(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        var salary = await _context.Salaries
                         .FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
                     ?? throw new ResourceNotFoundException("Salary record not found");

        salary.ExcludeFromStats();
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
        var query = _context.Salaries
            .When(request.CompanyType.HasValue, x => x.Company == request.CompanyType.Value)
            .When(request.Grade.HasValue, x => x.Grade == request.Grade.Value)
            .When(request.Profession.HasValue, x => x.ProfessionId == request.Profession.Value)
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
                ProfessionId = x.ProfessionId,
                City = x.City,
                Age = x.Age,
                YearOfStartingWork = x.YearOfStartingWork,
                Gender = x.Gender,
                SkillId = x.SkillId,
                WorkIndustryId = x.WorkIndustryId,
                CreatedAt = x.CreatedAt,
                UpdatedAt = x.UpdatedAt,
            })
            .AsNoTracking();

        switch (request.OrderType)
        {
            case GetAllSalariesOrderType.CreatedAtAsc:
                query = query.OrderBy(x => x.CreatedAt);
                break;

            case GetAllSalariesOrderType.CreatedAtDesc:
                query = query.OrderByDescending(x => x.CreatedAt);
                break;

            case GetAllSalariesOrderType.ValueAsc:
                query = query.OrderBy(x => x.Value);
                break;

            case GetAllSalariesOrderType.ValueDesc:
                query = query.OrderByDescending(x => x.Value);
                break;
        }

        return query;
    }
}