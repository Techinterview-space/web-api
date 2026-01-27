using System.Collections.Generic;
using System.Linq;
using Domain.Entities.Salaries;
using Domain.ValueObjects;
using Infrastructure.Database;
using Infrastructure.Salaries;
using Microsoft.EntityFrameworkCore;
using Web.Api.Features.Salaries.Models;

namespace Web.Api.Features.Salaries.Admin;

public record SalariesAdminQuery
{
    private readonly DatabaseContext _context;

    private IQueryable<UserSalary> _query;

    public SalariesAdminQuery(
        DatabaseContext context)
    {
        _context = context;
        _query = _context.Salaries.AsNoTracking();
    }

    public SalariesAdminQuery WithSource(
        params SalarySourceType[] sourceTypes)
    {
        return WithSource(sourceTypes.ToList());
    }

    public SalariesAdminQuery WithSource(
        List<SalarySourceType> sourceTypes)
    {
        if (sourceTypes.Count > 0)
        {
            _query = _query
                .Where(x =>
                    x.SourceType != null &&
                    sourceTypes.Contains(x.SourceType.Value));
        }

        return this;
    }

    public SalariesAdminQuery ApplyFilters(
        GetAllSalariesQueryParams queryParams)
    {
        _query = _query
            .When(queryParams.CompanyType.HasValue, x => x.Company == queryParams.CompanyType.Value)
            .When(queryParams.Grade.HasValue, x => x.Grade == queryParams.Grade.Value)
            .When(queryParams.Profession.HasValue, x => x.ProfessionId == queryParams.Profession.Value);

        return this;
    }

    public SalariesAdminQuery ApplyFilters(
        ISalariesChartQueryParams queryParams)
    {
        _query = _query
            .When(queryParams.Grade.HasValue, x => x.Grade == queryParams.Grade.Value)
            .When(queryParams.SelectedProfessionIds.Count > 0, x => x.ProfessionId.HasValue && queryParams.SelectedProfessionIds.Contains(x.ProfessionId.Value))
            .When(queryParams.Cities.Count > 0, x => x.City.HasValue && queryParams.Cities.Contains(x.City.Value))
            .When(queryParams.Skills.Count > 0, x => x.SkillId != null && queryParams.Skills.Contains(x.SkillId.Value));

        if (queryParams.QuarterTo != null && queryParams.YearTo != null)
        {
            _query = _query
                .Where(x =>
                    (x.Year == queryParams.YearTo.Value && x.Quarter <= queryParams.QuarterTo.Value) ||
                    x.Year < queryParams.YearTo.Value);
        }

        return this;
    }

    public SalariesAdminQuery ApplyOrder(
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

    public SalariesAdminQuery ApplyShowInStats(
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
                SourceType = x.SourceType,
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
                SourceType = x.SourceType,
                CreatedAt = x.CreatedAt,
                UpdatedAt = x.UpdatedAt,
            });
    }
}