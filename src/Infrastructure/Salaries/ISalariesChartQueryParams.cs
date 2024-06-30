using Domain.Entities.Enums;
using Domain.Entities.Salaries;
using Domain.Enums;

namespace Infrastructure.Salaries;

public interface ISalariesChartQueryParams
{
    public DeveloperGrade? Grade { get; }

    public List<long> ProfessionsToInclude { get; }

    public List<long> Skills { get; }

    public List<KazakhstanCity> Cities { get; }

    public SalarySourceType? SalarySourceType { get; }

    public int? QuarterTo { get; }

    public int? YearTo { get; }

    public bool HasAnyFilter =>
        Grade.HasValue || ProfessionsToInclude.Count > 0 || Cities.Count > 0;
}