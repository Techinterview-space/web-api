using System;
using MediatR;
using Web.Api.Features.Salaries.Models;

namespace Web.Api.Features.Salaries.UpdateSalary;

public record UpdateSalaryCommand : EditSalaryRequest, IRequest<CreateOrEditSalaryRecordResponse>
{
    public UpdateSalaryCommand()
    {
    }

    public UpdateSalaryCommand(
        Guid id,
        EditSalaryRequest request)
    {
        Id = id;
        Grade = request.Grade;
        YearOfStartingWork = request.YearOfStartingWork;
        Gender = request.Gender;
        ProfessionId = request.ProfessionId;
        Company = request.Company;
        City = request.City;
        SkillId = request.SkillId;
        WorkIndustryId = request.WorkIndustryId;
        Age = request.Age;
    }

    public Guid Id { get; init; }
}