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

        DevelopersByAgeChartData = new DevelopersByAgeChartData(salaries);
        DevelopersByExperienceYearsChartData = new DevelopersByExperienceYears(salaries);

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
            PeopleByGradesChartDataForLocal = new PeopleByGradesChartData(localSalaries);
        }

        if (remoteSalaries.Any())
        {
            var values = remoteSalaries
                .Select(x => x.Value)
                .ToList();

            AverageRemoteSalary = values.Average();
            MedianRemoteSalary = values.Median();

            SalariesByMoneyBarChartForRemote = new SalariesByMoneyBarChart(remoteSalaries);
            PeopleByGradesChartDataForRemote = new PeopleByGradesChartData(remoteSalaries);
        }

        DevelopersByGradeDistributionDataForLocal = new DevelopersByGradeDistributionData(localSalaries);
        DevelopersByGradeDistributionDataForRemote = new DevelopersByGradeDistributionData(remoteSalaries);

        ProfessionsDistributionDataForLocal = new ProfessionsDistributionData(localSalaries);
        ProfessionsDistributionDataForRemote = new ProfessionsDistributionData(remoteSalaries);
    }

    public static SalariesChartResponse RequireOwnSalary(
        List<double> salaryValues,
        int salariesCount,
        bool onlyLocalCompanySalaries,
        bool hasAuthentication)
    {
        return new (
            new List<UserSalaryDto>(),
            null,
            true,
            null,
            null,
            salariesCount)
        {
            AverageSalary = salaryValues.Count > 0 ? salaryValues.Average() : 0,
            MedianSalary = salaryValues.Count > 0 ? salaryValues.Median() : 0,
            OnlyLocalCompanySalaries = onlyLocalCompanySalaries,
            HasAuthentication = hasAuthentication,
        };
    }

    public List<UserSalaryDto> Salaries { get; }

    public int TotalCountInStats { get; }

    public UserSalaryAdminDto CurrentUserSalary { get; }

    public bool OnlyLocalCompanySalaries { get; private set; }

    public bool ShouldAddOwnSalary { get; }

    public bool HasAuthentication { get; private set; }

    public double AverageSalary { get; private set; }

    public double MedianSalary { get; private set; }

    public double? AverageRemoteSalary { get; }

    public double? MedianRemoteSalary { get; }

    public DateTimeOffset? RangeStart { get; }

    public DateTimeOffset? RangeEnd { get; }

    public SalariesByMoneyBarChart SalariesByMoneyBarChart { get; }

    public SalariesByMoneyBarChart SalariesByMoneyBarChartForRemote { get; }

    public DevelopersByGradeDistributionData DevelopersByGradeDistributionDataForLocal { get; }

    public DevelopersByGradeDistributionData DevelopersByGradeDistributionDataForRemote { get; }

    public ProfessionsDistributionData ProfessionsDistributionDataForLocal { get; }

    public ProfessionsDistributionData ProfessionsDistributionDataForRemote { get; }

    public PeopleByGradesChartData PeopleByGradesChartDataForLocal { get; }

    public PeopleByGradesChartData PeopleByGradesChartDataForRemote { get; }

    public DevelopersByAgeChartData DevelopersByAgeChartData { get; }

    public DevelopersByExperienceYears DevelopersByExperienceYearsChartData { get; }
}