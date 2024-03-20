using System;
using MediatR;
using TechInterviewer.Features.Salaries.Models;

namespace TechInterviewer.Features.Salaries.UpdateSalary;

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