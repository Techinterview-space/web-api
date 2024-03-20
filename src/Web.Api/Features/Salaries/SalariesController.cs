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
using Domain.Salaries;
using Domain.Services.Global;
using Domain.Services.Salaries;
using Domain.ValueObjects.Pagination;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SendGrid.Helpers.Errors.Model;
using TechInterviewer.Controllers.Labels;
using TechInterviewer.Controllers.Salaries;
using TechInterviewer.Controllers.Salaries.CreateSalaryRecord;
using TechInterviewer.Features.Salaries.Admin.GetApprovedSalaries;
using TechInterviewer.Features.Salaries.Admin.GetExcludedFromStatsSalaries;
using TechInterviewer.Features.Salaries.GetAdminChart;
using TechInterviewer.Features.Salaries.GetSalariesChart;
using TechInterviewer.Features.Salaries.GetSalariesChart.Charts;
using TechInterviewer.Setup.Attributes;

namespace TechInterviewer.Features.Salaries;

[ApiController]
[Route("api/salaries")]
public class SalariesController : ControllerBase
{
    private readonly IAuthorization _auth;
    private readonly DatabaseContext _context;
    private readonly IMediator _mediator;

    public SalariesController(
        IAuthorization auth,
        DatabaseContext context,
        IGlobal global,
        IMediator mediator)
    {
        _auth = auth;
        _context = context;
        _mediator = mediator;
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
        [FromQuery] GetApprovedSalariesQuery request,
        CancellationToken cancellationToken)
    {
        return await _mediator.Send(
            request,
            cancellationToken);
    }

    [HttpGet("not-in-stats")]
    [HasAnyRole(Role.Admin)]
    public async Task<Pageable<UserSalaryAdminDto>> AllNotShownInStatsAsync(
        [FromQuery] GetExcludedFromStatsSalariesQuery request,
        CancellationToken cancellationToken)
    {
        return await _mediator.Send(
            request,
            cancellationToken);
    }

    [HttpGet("salaries-adding-trend-chart")]
    [HasAnyRole(Role.Admin)]
    public async Task<AdminChartResponse> AdminChart(
        CancellationToken cancellationToken)
    {
        return await _mediator.Send(new GetAdminChartQuery(), cancellationToken);
    }

    [HttpGet("chart")]
    public Task<SalariesChartResponse> ChartAsync(
        [FromQuery] GetSalariesChartQuery request,
        CancellationToken cancellationToken)
    {
        return _mediator.Send(
            new GetSalariesChartQuery
            {
                Grade = request.Grade,
                ProfessionsToInclude = new DeveloperProfessionsCollection(request.ProfessionsToInclude).ToList(),
                Cities = request.Cities,
            },
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
}