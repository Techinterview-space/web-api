using Domain.Entities.Enums;
using Domain.Enums;

namespace Infrastructure.Salaries;

public interface ISalariesChartQueryParams
{
    public DeveloperGrade? Grade { get; }

    public List<long> ProfessionsToInclude { get; }

    public List<KazakhstanCity> Cities { get; }

    public bool HasAnyFilter =>
        Grade.HasValue || ProfessionsToInclude.Count > 0 || Cities.Count > 0;
}