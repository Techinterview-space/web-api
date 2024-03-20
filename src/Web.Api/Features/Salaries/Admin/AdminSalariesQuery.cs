using System.Linq;
using Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using TechInterviewer.Features.Salaries.Models;

namespace TechInterviewer.Features.Salaries.Admin;

public record AdminSalariesQuery
{
    private readonly DatabaseContext _context;

    public AdminSalariesQuery(
        DatabaseContext context)
    {
        _context = context;
    }

    public IQueryable<UserSalaryAdminDto> ToQueryable(
        GetAllSalariesQueryParams queryParams,
        bool? showInStats)
    {
        var query = _context.Salaries
            .When(queryParams.CompanyType.HasValue, x => x.Company == queryParams.CompanyType.Value)
            .When(queryParams.Grade.HasValue, x => x.Grade == queryParams.Grade.Value)
            .When(queryParams.Profession.HasValue, x => x.ProfessionId == queryParams.Profession.Value)
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

        switch (queryParams.OrderType)
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