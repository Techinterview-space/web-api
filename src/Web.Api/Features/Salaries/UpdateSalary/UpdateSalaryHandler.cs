using System.Threading;
using System.Threading.Tasks;
using Domain.Entities.Salaries;
using Domain.Entities.Users;
using Domain.Enums;
using Infrastructure.Authentication.Contracts;
using Infrastructure.Database;
using Infrastructure.Database.Extensions;
using Infrastructure.Salaries;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SendGrid.Helpers.Errors.Model;
using TechInterviewer.Features.Salaries.Models;
using NotFoundException = Domain.Validation.Exceptions.NotFoundException;

namespace TechInterviewer.Features.Salaries.UpdateSalary;

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

        var salary = await GetSalaryAsync(request.Id, cancellationToken);

        var currentUser = await _auth.CurrentUserOrNullAsync();

        CheckAuthorization(currentUser, salary);

        var skill = await GetEntityByIdAsync<Skill>(request.SkillId, cancellationToken);
        var workIndustry = await GetEntityByIdAsync<WorkIndustry>(request.WorkIndustryId, cancellationToken);
        var profession = await GetEntityByIdAsync<Profession>(request.ProfessionId, cancellationToken);

        UpdateSalaryRecord(salary, request, skill, workIndustry, profession);

        await _context.SaveChangesAsync(cancellationToken);
        return CreateOrEditSalaryRecordResponse.Success(new UserSalaryDto(salary));
    }

    private async Task<T> GetEntityByIdAsync<T>(int? id, CancellationToken cancellationToken) where T : class
    {
        if (id is null || id <= 0)
            return null;

        return await _context.Set<T>().FirstOrDefaultAsync(x => x.Id == id.Value, cancellationToken);
    }

    private void CheckAuthorization(User currentUser, Salary salary)
    {
        if (!currentUser.Has(Role.Admin) && salary.UserId != currentUser.Id)
            throw new ForbiddenException("You can only edit your own salary records");
    }

    private async Task<Salary> GetSalaryAsync(int salaryId, CancellationToken cancellationToken)
    {
        var salary = await _context.Salaries
            .FirstOrDefaultAsync(x => x.Id == salaryId, cancellationToken);

        if (salary == null)
            throw new NotFoundException("Salary record not found");

        return salary;
    }

    private void UpdateSalaryRecord(Salary salary, UpdateSalaryCommand request, Skill skill, WorkIndustry workIndustry, Profession profession)
    {
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
    }
}