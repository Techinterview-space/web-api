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
        UserSalaryAdminDto currentUserSalary,
        DateTimeOffset? rangeStart,
        DateTimeOffset? rangeEnd,
        int totalCountInStats)
        : this(
            salaries,
            currentUserSalary,
            false,
            rangeStart,
            rangeEnd,
            totalCountInStats)
    {
    }

    private SalariesChartResponse(
        List<UserSalaryDto> salaries,
        UserSalaryAdminDto currentUserSalary,
        bool shouldAddOwnSalary,
        DateTimeOffset? rangeStart,
        DateTimeOffset? rangeEnd,
        int totalCountInStats)
    {
        Salaries = salaries;
        CurrentUserSalary = currentUserSalary;
        ShouldAddOwnSalary = shouldAddOwnSalary;
        RangeStart = rangeStart;
        RangeEnd = rangeEnd;
        TotalCountInStats = totalCountInStats;

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

    public static SalariesChartResponse RequireOwnSalary(
        int totalCountInStats)
    {
        return new (
            new List<UserSalaryDto>(),
            null,
            true,
            null,
            null,
            totalCountInStats);
    }

    public List<UserSalaryDto> Salaries { get; }

    public int TotalCountInStats { get; }

    public UserSalaryAdminDto CurrentUserSalary { get; }

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