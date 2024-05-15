using System.Linq.Expressions;
using Domain.Entities.Enums;
using Domain.Entities.Salaries;
using Domain.Enums;
using Domain.Extensions;
using Domain.ValueObjects.Dates;
using Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Salaries;

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
        ProfessionsToInclude = professionsToInclude ?? new List<long>();
        Cities = cities ?? new List<KazakhstanCity>();

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
            .When(Grade.HasValue, x => x.Grade == Grade.Value);

        if (Cities.Count != 0)
        {
            if (Cities.Count == 1 && Cities[0] == KazakhstanCity.Undefined)
            {
                query = query.Where(s => s.City == null);
            }

            Expression<Func<UserSalary, bool>> clause = s => s.City != null && Cities.Contains(s.City.Value);
            if (Cities.Any(x => x == KazakhstanCity.Undefined))
            {
                clause = clause.Or(x => x.City == null);
            }

            query = query.Where(clause);
        }

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
                CreatedAt = x.CreatedAt,
            })
            .OrderBy(x => x.Value)
            .AsNoTracking();
    }
}