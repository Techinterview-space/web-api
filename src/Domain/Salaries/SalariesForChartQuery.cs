using System;
using System.Collections.Generic;
using System.Linq;
using Domain.Database;
using Domain.Entities.Enums;
using Domain.Entities.Salaries;
using Domain.Enums;
using Domain.Extensions;
using Domain.Services.Salaries;
using Domain.ValueObjects.Dates;
using Microsoft.EntityFrameworkCore;

namespace Domain.Salaries;

public record SalariesForChartQuery
{
    public DateQuarter CurrentQuarter { get; }

    public DeveloperGrade? Grade { get; }

    public List<long> ProfessionsToInclude { get; }

    public List<KazakhstanCity> Cities { get; }

    public DateTimeOffset SalaryAddedEdge { get; }

    private readonly DatabaseContext _context;

    public SalariesForChartQuery(
        DatabaseContext context,
        DeveloperGrade? grade,
        List<long> professionsToInclude,
        List<KazakhstanCity> cities)
    {
        _context = context;
        Grade = grade;
        ProfessionsToInclude = professionsToInclude;
        Cities = cities;

        CurrentQuarter = DateQuarter.Current;
        SalaryAddedEdge = DateTimeOffset.Now.AddMonths(-6);
    }

    public SalariesForChartQuery(
        DatabaseContext context,
        ISalariesChartQueryParams request)
        : this(
            context,
            request.Grade,
            request.ProfessionsToInclude,
            request.Cities)
    {
    }

    public IQueryable<UserSalaryDto> ToQueryable(
        CompanyType? companyType = null)
    {
        var query = _context.Salaries
            .Where(x => x.UseInStats)
            .Where(x => x.ProfessionId != (long)UserProfessionEnum.HrNonIt)
            .Where(x => x.Year == CurrentQuarter.Year || x.Year == CurrentQuarter.Year - 1)
            .Where(x => x.CreatedAt >= SalaryAddedEdge)
            .When(companyType.HasValue, x => x.Company == companyType.Value)
            .FilterByCitiesIfNecessary(Cities)
            .When(Grade.HasValue, x => x.Grade == Grade.Value);

        query = query
            .When(
                ProfessionsToInclude.Count > 0,
                x => x.ProfessionId != null && ProfessionsToInclude.Contains(x.ProfessionId.Value));

        return query
            .Select(x => new UserSalaryDto
            {
                Value = x.Value,
                Quarter = x.Quarter,
                Year = x.Year,
                Currency = x.Currency,
                Company = x.Company,
                Grade = x.Grade,
                City = x.City,
                Age = x.Age,
                YearOfStartingWork = x.YearOfStartingWork,
                Gender = x.Gender,
                SkillId = x.SkillId,
                WorkIndustryId = x.WorkIndustryId,
                ProfessionId = x.ProfessionId,
            })
            .OrderBy(x => x.Value)
            .AsNoTracking();
    }
}