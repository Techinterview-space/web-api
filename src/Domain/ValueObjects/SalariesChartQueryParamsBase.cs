using System;
using System.Collections.Generic;
using Domain.Entities.Enums;
using Domain.Entities.Salaries;
using Domain.Enums;

namespace Domain.ValueObjects;

public record SalariesChartQueryParamsBase : ISalariesChartQueryParams
{
    public SalariesChartQueryParamsBase()
    {
    }

    public SalariesChartQueryParamsBase(
        DeveloperGrade? grade,
        List<long> selectedProfessionIds,
        List<long> skills,
        List<KazakhstanCity> cities,
        List<SalarySourceType> salarySourceTypes,
        int? quarterTo,
        int? yearTo,
        DateTime? dateTo)
    {
        Grade = grade;
        SelectedProfessionIds = selectedProfessionIds;
        Skills = skills;
        Cities = cities;
        SalarySourceTypes = salarySourceTypes;
        QuarterTo = quarterTo;
        YearTo = yearTo;
        DateTo = dateTo;
    }

    public virtual DeveloperGrade? Grade { get; init; }

    public virtual List<long> SelectedProfessionIds { get; init; }

    public virtual List<long> Skills { get; init; }

    public virtual List<KazakhstanCity> Cities { get; init; }

    public virtual List<SalarySourceType> SalarySourceTypes { get; init; }

    public virtual int? QuarterTo { get; init; }

    public virtual int? YearTo { get; init; }

    public virtual DateTime? DateTo { get; init; }

    public virtual bool HasAnyFilter =>
        Grade.HasValue || SelectedProfessionIds.Count > 0 || Cities.Count > 0;

    public virtual string GetKeyPostfix()
    {
        var grade = Grade?.ToString() ?? "all";
        var professions = SelectedProfessionIds.Count == 0 ? "all" : string.Join("_", SelectedProfessionIds);
        return $"{grade}_{professions}";
    }
}