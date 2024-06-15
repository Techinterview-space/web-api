using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Domain.Entities.Salaries;
using Infrastructure.Authentication.Contracts;
using Infrastructure.Database;
using Infrastructure.Salaries;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Web.Api.Features.Salaries.Models;

namespace Web.Api.Features.Salaries.AddSalary;

public class AddSalaryHandler : IRequestHandler<AddSalaryCommand, CreateOrEditSalaryRecordResponse>
{
    private readonly IAuthorization _auth;
    private readonly DatabaseContext _context;

    public AddSalaryHandler(
        IAuthorization auth,
        DatabaseContext context)
    {
        _auth = auth;
        _context = context;
    }

    public async Task<CreateOrEditSalaryRecordResponse> Handle(
        AddSalaryCommand request,
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
}