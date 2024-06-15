using System.Linq;
using Domain.Entities.Salaries;
using Infrastructure.Database;
using Infrastructure.Salaries;
using Microsoft.EntityFrameworkCore;
using Web.Api.Features.Salaries.Models;

namespace Web.Api.Features.Salaries.Admin;

public record SalariesQuery
{
    private readonly DatabaseContext _context;

    private IQueryable<UserSalary> _query;

    public SalariesQuery(
        DatabaseContext context)
    {
        _context = context;
        _query = _context.Salaries.AsNoTracking();
    }

    public SalariesQuery ApplyFilters(
        GetAllSalariesQueryParams queryParams)
    {
        _query = _query
            .When(queryParams.CompanyType.HasValue, x => x.Company == queryParams.CompanyType.Value)
            .When(queryParams.Grade.HasValue, x => x.Grade == queryParams.Grade.Value)
            .When(queryParams.Profession.HasValue, x => x.ProfessionId == queryParams.Profession.Value);

        return this;
    }

    public SalariesQuery ApplyFilters(
        ISalariesChartQueryParams queryParams)
    {
        _query = _query
            .When(queryParams.Grade.HasValue, x => x.Grade == queryParams.Grade.Value)
            .When(queryParams.ProfessionsToInclude.Count > 0, x => x.ProfessionId.HasValue && queryParams.ProfessionsToInclude.Contains(x.ProfessionId.Value))
            .When(queryParams.Cities.Count > 0, x => x.City.HasValue && queryParams.Cities.Contains(x.City.Value));

        return this;
    }

    public SalariesQuery ApplyOrder(
        GetAllSalariesOrderType orderType)
    {
        switch (orderType)
        {
            case GetAllSalariesOrderType.CreatedAtAsc:
                _query = _query.OrderBy(x => x.CreatedAt);
                break;

            case GetAllSalariesOrderType.CreatedAtDesc:
                _query = _query.OrderByDescending(x => x.CreatedAt);
                break;

            case GetAllSalariesOrderType.ValueAsc:
                _query = _query.OrderBy(x => x.Value);
                break;

            case GetAllSalariesOrderType.ValueDesc:
                _query = _query.OrderByDescending(x => x.Value);
                break;
        }

        return this;
    }

    public SalariesQuery ApplyShowInStats(
        bool? showInStats)
    {
        _query = _query
            .When(showInStats.HasValue, x => x.UseInStats == showInStats.Value);

        return this;
    }

    public IQueryable<UserSalaryAdminDto> ToAdminDtoQueryable()
    {
        return _query
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
            });
    }

    public IQueryable<UserSalaryDto> ToPublicDtoQueryable()
    {
        return _query
            .Select(x => new UserSalaryDto
            {
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
            });
    }
}