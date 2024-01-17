using System;
using System.Collections.Generic;
using System.Linq;
using Domain.Entities.Salaries;
using Domain.Extensions;
using Domain.Services.Salaries;

namespace TechInterviewer.Controllers.Salaries.Charts;

public record SalariesChartResponse
{
    public SalariesChartResponse(
        List<UserSalaryDto> salaries,
        DateTimeOffset? rangeStart,
        DateTimeOffset? rangeEnd)
        : this(
            salaries,
            false,
            rangeStart,
            rangeEnd)
    {
    }

    private SalariesChartResponse(
        List<UserSalaryDto> salaries,
        bool shouldAddOwnSalary,
        DateTimeOffset? rangeStart,
        DateTimeOffset? rangeEnd)
    {
        Salaries = salaries;
        ShouldAddOwnSalary = shouldAddOwnSalary;
        RangeStart = rangeStart;
        RangeEnd = rangeEnd;

        var localSalaries = salaries
            .Where(x => x.Company == CompanyType.Local)
            .ToList();

        var remoteSalaries = salaries
            .Where(x => x.Company == CompanyType.Foreign)
            .ToList();

        if (localSalaries.Any())
        {
            AverageSalary = localSalaries.Select(x => x.Value).Average();
            MedianSalary = localSalaries.Select(x => x.Value).Median();

            SalariesByMoneyBarChart = new SalariesByMoneyBarChart(localSalaries);
        }

        if (remoteSalaries.Any())
        {
            var values = remoteSalaries
                .Select(x => x.Value)
                .ToList();

            AverageRemoteSalary = values.Average();
            MedianRemoteSalary = values.Median();

            SalariesByMoneyBarChartForRemote = new SalariesByMoneyBarChart(remoteSalaries);
        }
    }

    public static SalariesChartResponse RequireOwnSalary()
    {
        return new (
            new List<UserSalaryDto>(),
            true,
            null,
            null);
    }

    public List<UserSalaryDto> Salaries { get; }

    public bool ShouldAddOwnSalary { get; }

    public double AverageSalary { get; }

    public double MedianSalary { get; }

    public double? AverageRemoteSalary { get; }

    public double? MedianRemoteSalary { get; }

    public DateTimeOffset? RangeStart { get; }

    public DateTimeOffset? RangeEnd { get; }

    public SalariesByMoneyBarChart SalariesByMoneyBarChart { get; }

    public SalariesByMoneyBarChart SalariesByMoneyBarChartForRemote { get; }
}