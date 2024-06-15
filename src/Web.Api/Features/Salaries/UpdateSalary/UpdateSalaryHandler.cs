using System.Threading;
using System.Threading.Tasks;
using Domain.Entities.Salaries;
using Domain.Enums;
using Infrastructure.Authentication.Contracts;
using Infrastructure.Database;
using Infrastructure.Salaries;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SendGrid.Helpers.Errors.Model;
using Web.Api.Features.Salaries.Models;
using NotFoundException = Domain.Validation.Exceptions.NotFoundException;

namespace Web.Api.Features.Salaries.UpdateSalary;

public class UpdateSalaryHandler : IRequestHandler<UpdateSalaryCommand, CreateOrEditSalaryRecordResponse>
{
    private readonly DatabaseContext _context;
    private readonly IAuthorization _auth;

    public UpdateSalaryHandler(
        DatabaseContext context,
        IAuthorization auth)
    {
        _context = context;
        _auth = auth;
    }

    public async Task<CreateOrEditSalaryRecordResponse> Handle(
        UpdateSalaryCommand request,
        CancellationToken cancellationToken)
    {
        request.IsValidOrFail();
        var salary = await _context.Salaries
                         .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken)
                     ?? throw new NotFoundException("Salary record not found");

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
}