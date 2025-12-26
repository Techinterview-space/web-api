using System;
using System.Collections.Generic;
using Domain.Entities.Enums;
using Domain.Entities.Salaries;
using Domain.Enums;

namespace Domain.ValueObjects;

public interface ISalariesChartQueryParams
{
    public DeveloperGrade? Grade { get; }

    public List<long> SelectedProfessionIds { get; }

    public List<long> Skills { get; }

    public List<KazakhstanCity> Cities { get; }

    public List<SalarySourceType> SalarySourceTypes { get; }

    public int? QuarterTo { get; }

    public int? YearTo { get; }

    public DateTime? DateTo { get; }
}