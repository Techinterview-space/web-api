using System.Linq.Expressions;
using Domain.Entities.Enums;
using Domain.Entities.Salaries;
using Domain.Enums;
using Domain.Extensions;
using Domain.ValueObjects;
using Domain.ValueObjects.Dates;
using Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Salaries;

public record SalariesForChartQuery
{
    public const int MonthsToShow = 18;

    public DateQuarter CurrentQuarter { get; }

    public DeveloperGrade? Grade { get; }

    public List<long> Skills { get; }

    public List<KazakhstanCity> Cities { get; }

    public DateTimeOffset From { get; }

    public DateTimeOffset To { get; }

    public List<SalarySourceType> SalarySourceTypes { get; }

    public int? QuarterTo { get; }

    public int? YearTo { get; }

    private readonly List<long> _selectedProfessionIds;
    private readonly DatabaseContext _context;

    private IQueryable<UserSalary> _query;

    public SalariesForChartQuery(
        DatabaseContext context,
        DeveloperGrade? grade,
        List<long> professionsToInclude,
        List<long> skills,
        List<KazakhstanCity> cities,
        DateTimeOffset from,
        DateTimeOffset to,
        List<SalarySourceType> salarySourceTypes,
        int? quarterTo,
        int? yearTo)
    {
        _context = context;
        Grade = grade;
        _selectedProfessionIds = professionsToInclude ?? new List<long>();
        Skills = skills ?? new List<long>();

        Cities = cities ?? new List<KazakhstanCity>();

        CurrentQuarter = DateQuarter.Current;
        From = from;
        To = to;

        SalarySourceTypes = salarySourceTypes ?? new List<SalarySourceType>();
        QuarterTo = quarterTo;
        YearTo = yearTo;

        _query = BuildQuery();
    }

    public SalariesForChartQuery(
        DatabaseContext context,
        ISalariesChartQueryParams request,
        DateTimeOffset now)
        : this(
            context,
            request,
            now.AddMonths(-MonthsToShow),
            now)
    {
    }

    public SalariesForChartQuery(
        DatabaseContext context,
        ISalariesChartQueryParams request,
        DateTimeOffset from,
        DateTimeOffset to)
        : this(
            context,
            request.Grade,
            request.SelectedProfessionIds,
            request.Skills,
            request.Cities,
            from,
            to,
            request.SalarySourceTypes,
            request.QuarterTo,
            request.YearTo)
    {
    }

    public SalariesForChartQuery Where(
        Expression<Func<UserSalary, bool>> predicate)
    {
        _query = _query.Where(predicate);
        return this;
    }

    public IQueryable<UserSalaryDto> ToQueryable(
        CompanyType? companyType = null)
    {
        return _query
            .When(companyType.HasValue, x => x.Company == companyType.Value)
            .OrderBy(x => x.Value)
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
            .AsNoTracking();
    }

    public IQueryable<TResult> ToQueryable<TResult>(
        Expression<Func<UserSalary, TResult>> selector,
        CompanyType? companyType = null)
    {
        return _query
            .When(companyType.HasValue, x => x.Company == companyType.Value)
            .OrderBy(x => x.Value)
            .Select(selector);
    }

    private IQueryable<UserSalary> BuildQuery()
    {
        var query = _context.Salaries
            .Where(x => x.UseInStats)
            .Where(x => x.ProfessionId != (long)UserProfessionEnum.HrNonIt)
            .When(Grade.HasValue, x => x.Grade == Grade.Value)
            .When(SalarySourceTypes is { Count: > 0 }, x =>
                x.SourceType != null &&
                SalarySourceTypes.Contains(x.SourceType.Value));

        if (QuarterTo.HasValue && YearTo.HasValue)
        {
            query = query
                .Where(x =>
                    (x.Year == YearTo.Value && x.Quarter <= QuarterTo.Value) ||
                    x.Year < YearTo.Value);
        }
        else if (SalarySourceTypes.Count == 0)
        {
            query = query
                .Where(x =>
                    (x.Year == CurrentQuarter.Year || x.Year == CurrentQuarter.Year - 1) &&
                    x.CreatedAt >= From && x.CreatedAt <= To);
        }

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
                Skills.Count > 0,
                x => x.SkillId != null && Skills.Contains(x.SkillId.Value))
            .When(
                _selectedProfessionIds.Count > 0,
                x => x.ProfessionId != null && _selectedProfessionIds.Contains(x.ProfessionId.Value));

        return query;
    }

    public Task<int> CountAsync(
        CancellationToken cancellationToken)
    {
        return ToQueryable().CountAsync(cancellationToken);
    }
}