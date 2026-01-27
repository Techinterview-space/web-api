using System;
using System.Collections.Generic;
using System.Linq;
using Domain.Entities.Currencies;
using Domain.Entities.Enums;
using Domain.Entities.Salaries;
using Domain.Extensions;
using Infrastructure.Currencies.Contracts;
using Infrastructure.Salaries;
using Web.Api.Features.Salaries.Models;

namespace Web.Api.Features.Salaries.GetSalariesChart.Charts;

public record SalariesChartResponse
{
    private static readonly List<DeveloperGrade> _gradesToBeUsedInChart = new ()
    {
        DeveloperGrade.Junior,
        DeveloperGrade.Middle,
        DeveloperGrade.Senior,
        DeveloperGrade.Lead,
    };

    public SalariesChartResponse(
        List<UserSalaryDto> salaries,
        UserSalaryAdminDto currentUserSalary,
        bool hasSurveyRecentReply,
        DateTimeOffset? rangeStart,
        DateTimeOffset? rangeEnd,
        List<CurrencyContent> currencies,
        SalariesSkillsChartData salariesSkillsChartData = null,
        WorkIndustriesChartData workIndustriesChartData = null,
        CitiesDoughnutChartData citiesDoughnutChartData = null,
        GradesMinMaxChartData gradesMinMaxChartData = null,
        ProfessionsDistributionChartData professionsDistributionChartData = null,
        PeopleByGenderChartData peopleByGenderChartData = null)
        : this(
            salaries,
            currentUserSalary,
            hasSurveyRecentReply,
            false,
            rangeStart,
            rangeEnd,
            salaries.Count,
            true,
            currencies,
            salariesSkillsChartData,
            workIndustriesChartData,
            citiesDoughnutChartData,
            gradesMinMaxChartData,
            professionsDistributionChartData,
            peopleByGenderChartData)
    {
    }

    private SalariesChartResponse(
        List<UserSalaryDto> salaries,
        UserSalaryAdminDto currentUserSalary,
        bool hasSurveyRecentReply,
        bool shouldAddOwnSalary,
        DateTimeOffset? rangeStart,
        DateTimeOffset? rangeEnd,
        int totalCountInStats,
        bool hasAuthentication,
        List<CurrencyContent> currencies,
        SalariesSkillsChartData salariesSkillsChartData = null,
        WorkIndustriesChartData workIndustriesChartData = null,
        CitiesDoughnutChartData citiesDoughnutChartData = null,
        GradesMinMaxChartData gradesMinMaxChartData = null,
        ProfessionsDistributionChartData professionsDistributionChartData = null,
        PeopleByGenderChartData peopleByGenderChartData = null)
    {
        SalariesCount = salaries?.Count ?? 0;
        Currencies = currencies;

        CurrentUserSalary = currentUserSalary;
        HasRecentSurveyReply = hasSurveyRecentReply;
        ShouldAddOwnSalary = shouldAddOwnSalary;
        RangeStart = rangeStart;
        RangeEnd = rangeEnd;
        TotalCountInStats = totalCountInStats;
        HasAuthentication = hasAuthentication;

        // Set the new chart data properties
        SalariesSkillsChartData = salariesSkillsChartData;
        WorkIndustriesChartData = workIndustriesChartData;
        CitiesDoughnutChartData = citiesDoughnutChartData;
        GradesMinMaxChartData = gradesMinMaxChartData;
        ProfessionsDistributionChartData = professionsDistributionChartData;
        PeopleByGenderChartData = peopleByGenderChartData;

        DevelopersByAgeChartData = new DevelopersByAgeChartData(salaries);
        DevelopersByExperienceYearsChartData = new DevelopersByExperienceYears(salaries);

        var localSalaries = salaries
            .Where(x => x.Company == CompanyType.Local)
            .ToList();

        var remoteSalaries = salaries
            .Where(x => x.Company == CompanyType.Foreign)
            .ToList();

        if (localSalaries.Count != 0)
        {
            var localSalaryValues = localSalaries
                .Select(x => x.Value)
                .Order()
                .ToList();

            AverageSalary = localSalaryValues.Average();
            MedianSalary = localSalaryValues.Median();

            LocalSalariesByGrade = _gradesToBeUsedInChart
                .Select(x => new MedianAndAverageSalariesByGrade(
                    x,
                    localSalaries
                        .Where(y => y.Grade == x)
                        .ToList()))
                .ToList();

            SalariesByMoneyBarChart = new SalariesByMoneyBarChart(localSalaryValues);
            PeopleByGradesChartDataForLocal = new PeopleByGradesChartData(localSalaries);
        }

        if (remoteSalaries.Count != 0)
        {
            var values = remoteSalaries
                .Select(x => x.Value)
                .ToList();

            AverageRemoteSalary = values.Average();
            MedianRemoteSalary = values.Median();

            RemoteSalariesByGrade = _gradesToBeUsedInChart
                .Select(x => new MedianAndAverageSalariesByGrade(
                    x,
                    remoteSalaries
                        .Where(y => y.Grade == x)
                        .ToList()))
                .ToList();

            SalariesByMoneyBarChartForRemote = new SalariesByMoneyBarChart(values);
            PeopleByGradesChartDataForRemote = new PeopleByGradesChartData(remoteSalaries);
        }

        DevelopersByGradeDistributionDataForLocal = new DevelopersByGradeDistributionData(localSalaries);
        DevelopersByGradeDistributionDataForRemote = new DevelopersByGradeDistributionData(remoteSalaries);

        ProfessionsDistributionDataForLocal = new ProfessionsDistributionData(localSalaries);
        ProfessionsDistributionDataForRemote = new ProfessionsDistributionData(remoteSalaries);

        SalariesByExperienceChartForLocalSalaries = new SalariesByExperienceChart(localSalaries);
        SalariesByExperienceChartForRemoteSalaries = new SalariesByExperienceChart(remoteSalaries);

        SalariesByUserAgeChartForLocalSalaries = new SalariesByUserAgeChart(localSalaries);
        SalariesByUserAgeChartForRemoteSalaries = new SalariesByUserAgeChart(remoteSalaries);

        SalariesByCityChartForLocal = new SalariesByCityChart(localSalaries);
        SalariesByCityChartForRemote = new SalariesByCityChart(remoteSalaries);

        SalariesByGenderChartForLocal = new SalariesByGenderChart(localSalaries);
        SalariesByGenderChartForRemote = new SalariesByGenderChart(remoteSalaries);
    }

    public static SalariesChartResponse RequireOwnSalary(
        List<(CompanyType Company, double Value)> salaryValues,
        int salariesCount,
        bool onlyLocalCompanySalaries,
        bool hasAuthentication)
    {
        var local = salaryValues
            .Where(x => x.Company == CompanyType.Local)
            .Select(x => x.Value)
            .ToList();

        var remote = salaryValues
            .Where(x => x.Company == CompanyType.Foreign)
            .Select(x => x.Value)
            .ToList();

        return new (
            new List<UserSalaryDto>(),
            null,
            false,
            true,
            null,
            null,
            salariesCount,
            hasAuthentication,
            new List<CurrencyContent>(0),
            null, // SalariesSkillsChartData
            null, // WorkIndustriesChartData
            null, // CitiesDoughnutChartData
            null, // GradesMinMaxChartData
            null, // ProfessionsDistributionChartData
            null) // PeopleByGenderChartData
        {
            AverageSalary = local.Count > 0 ? local.Average() : 0,
            MedianSalary = local.Count > 0 ? local.Median() : 0,
            MedianRemoteSalary = remote.Count > 0 ? remote.Median() : 0,
            AverageRemoteSalary = remote.Count > 0 ? remote.Average() : 0,
            LocalSalariesByGrade = new List<MedianAndAverageSalariesByGrade>(),
            RemoteSalariesByGrade = new List<MedianAndAverageSalariesByGrade>(),
            OnlyLocalCompanySalaries = onlyLocalCompanySalaries,
        };
    }

    public int SalariesCount { get; }

    public List<CurrencyContent> Currencies { get; }

    public int TotalCountInStats { get; }

    public UserSalaryAdminDto CurrentUserSalary { get; }

    public bool OnlyLocalCompanySalaries { get; private set; }

    public bool HasRecentSurveyReply { get; }

    public bool ShouldAddOwnSalary { get; }

    public bool HasAuthentication { get; }

    public double AverageSalary { get; private set; }

    public double MedianSalary { get; private set; }

    public List<MedianAndAverageSalariesByGrade> LocalSalariesByGrade { get; private set; }

    public List<MedianAndAverageSalariesByGrade> RemoteSalariesByGrade { get; private set; }

    public double? AverageRemoteSalary { get; private set; }

    public double? MedianRemoteSalary { get; private set; }

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

    public SalariesByExperienceChart SalariesByExperienceChartForLocalSalaries { get; }

    public SalariesByExperienceChart SalariesByExperienceChartForRemoteSalaries { get; }

    public SalariesByUserAgeChart SalariesByUserAgeChartForLocalSalaries { get; }

    public SalariesByUserAgeChart SalariesByUserAgeChartForRemoteSalaries { get; }

    public SalariesByCityChart SalariesByCityChartForLocal { get; }

    public SalariesByCityChart SalariesByCityChartForRemote { get; }

    public SalariesByGenderChart SalariesByGenderChartForLocal { get; }

    public SalariesByGenderChart SalariesByGenderChartForRemote { get; }

    public SalariesSkillsChartData SalariesSkillsChartData { get; }

    public WorkIndustriesChartData WorkIndustriesChartData { get; }

    public CitiesDoughnutChartData CitiesDoughnutChartData { get; }

    public GradesMinMaxChartData GradesMinMaxChartData { get; }

    public ProfessionsDistributionChartData ProfessionsDistributionChartData { get; }

    public PeopleByGenderChartData PeopleByGenderChartData { get; }
}