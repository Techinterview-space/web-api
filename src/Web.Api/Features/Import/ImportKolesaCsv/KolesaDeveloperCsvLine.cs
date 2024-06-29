using System;
using System.Collections.Generic;
using System.Linq;
using CsvHelper.Configuration.Attributes;
using Domain.Entities.Enums;
using Domain.Entities.Salaries;
using Domain.Enums;
using Domain.Extensions;

namespace Web.Api.Features.Import.ImportKolesaCsv;

public record KolesaDeveloperCsvLine
{
    public int RowId { get; set; }

    public string City { get; set; }

    public KazakhstanCity? CityAsEnum
    {
        get
        {
            if (string.IsNullOrEmpty(City))
            {
                return null;
            }

            var value = City.ToEnum<KazakhstanCity>();
            if (value is KazakhstanCity.Undefined)
            {
                return null;
            }

            return value;
        }
    }

    public string Gender { get; set; }

    public Gender? GenderAsEnum => Gender.ToEnum<Gender>();

    public int Age { get; set; }

    public string Education { get; set; }

    [BooleanTrueValues("TRUE")]
    [BooleanFalseValues("FALSE")]
    public bool UseInStat { get; set; }

    public int Experience { get; set; }

    public string WorkIndustry { get; set; }

    public string CompanyType { get; set; }

    public CompanyType? CompanyTypeAsEnum => CompanyType.ToEnum<CompanyType>();

    public string CompanyCountry { get; set; }

    public string Grade { get; set; }

    public DeveloperGrade? DeveloperGradeAsEnum => Grade.ToEnum<DeveloperGrade>();

    public string OperationSystem { get; set; }

    public string ProjectMethodology { get; set; }

    public string TaskManagement { get; set; }

    public string PreviousProfession { get; set; }

    public string CurrentProfession { get; set; }

    public string BackendLanguage { get; set; }

    public string BackendFramework { get; set; }

    public string Databases { get; set; }

    public string FrontendFramework { get; set; }

    public string MobileFramework { get; set; }

    public string WhatYouTest { get; set; }

    public string HasAutotests { get; set; }

    public string AutotestLanguage { get; set; }

    public string AutotestFramework { get; set; }

    public int? SalaryNett { get; set; }

    [BooleanTrueValues("TRUE")]
    [BooleanFalseValues("FALSE")]
    public bool IsYourSalaryInMarket { get; set; }

    public string AreYouReadyToChangeJob { get; set; }

    public int HowManyTimesDidYouChangeYourJobDuringLastFiveYears { get; set; }

    [Format("dd.MM.yyyy HH:mm:ss")]
    [Optional]
    public DateTime? SubmittedAt { get; set; }

    public UserSalary CreateUserSalary(
        List<Skill> skills,
        List<WorkIndustry> workIndustries,
        List<Profession> professions)
    {
        if (!UseInStat ||
            SalaryNett == null ||
            CompanyTypeAsEnum == null)
        {
            throw new InvalidOperationException("Salary record is not valid");
        }

        Skill skillOrNull = null;
        WorkIndustry workIndustryOrNull = null;
        Profession professionOrNull = null;

        if (!string.IsNullOrEmpty(BackendLanguage))
        {
            var backendLanguages = BackendLanguage
                .ToLowerInvariant()
                .Split(";")
                .Where(x => !string.IsNullOrEmpty(x))
                .ToList();

            skillOrNull = skills.FirstOrDefault(x =>
                backendLanguages.Any(y =>
                    x.Title.Contains(y, StringComparison.InvariantCultureIgnoreCase)));
        }
        else if (!string.IsNullOrEmpty(BackendFramework))
        {
            var backendFramework = BackendFramework
                .ToLowerInvariant()
                .Split(";")
                .Where(x => !string.IsNullOrEmpty(x))
                .ToList();

            skillOrNull = skills.FirstOrDefault(x =>
                backendFramework.Any(y =>
                    x.Title.Contains(y, StringComparison.InvariantCultureIgnoreCase)));
        }
        else if (!string.IsNullOrEmpty(FrontendFramework))
        {
            var frontendFramework = FrontendFramework
                .ToLowerInvariant()
                .Split(";")
                .Where(x => !string.IsNullOrEmpty(x))
                .ToList();

            skillOrNull = skills.FirstOrDefault(x =>
                frontendFramework.Any(y =>
                    x.Title.Contains(y, StringComparison.InvariantCultureIgnoreCase)));
        }
        else if (!string.IsNullOrEmpty(MobileFramework))
        {
            var mobileFramework = MobileFramework
                .ToLowerInvariant()
                .Split(";")
                .Where(x => !string.IsNullOrEmpty(x))
                .ToList();

            skillOrNull = skills.FirstOrDefault(x =>
                mobileFramework.Any(y =>
                    x.Title.Contains(y, StringComparison.InvariantCultureIgnoreCase)));
        }
        else if (!string.IsNullOrEmpty(AutotestLanguage))
        {
            var autotestLanguage = AutotestLanguage
                .ToLowerInvariant()
                .Split(";")
                .Where(x => !string.IsNullOrEmpty(x))
                .ToList();

            skillOrNull = skills.FirstOrDefault(x =>
                autotestLanguage.Any(y =>
                    x.Title.Contains(y, StringComparison.InvariantCultureIgnoreCase)));
        }
        else if (!string.IsNullOrEmpty(AutotestFramework))
        {
            var autotestFramework = AutotestFramework
                .ToLowerInvariant()
                .Split(";")
                .Where(x => !string.IsNullOrEmpty(x))
                .ToList();

            skillOrNull = skills.FirstOrDefault(x =>
                autotestFramework.Any(y =>
                    x.Title.Contains(y, StringComparison.InvariantCultureIgnoreCase)));
        }

        if (WorkIndustry != null)
        {
            var workIndustry = WorkIndustry.ToLowerInvariant();
            workIndustryOrNull = workIndustries.FirstOrDefault(x =>
                x.Title.Contains(workIndustry, StringComparison.InvariantCultureIgnoreCase));
        }

        var teamleadProfession = professions.FirstOrDefault(
            x => x.Title.Contains("Teamleader", StringComparison.InvariantCultureIgnoreCase));
        var techleadProfession = professions.FirstOrDefault(
            x => x.Title.Contains("Techleader", StringComparison.InvariantCultureIgnoreCase));
        var ctoProfession = professions.FirstOrDefault(
            x => x.Title.Equals("Chief Technical Officer (CTO)", StringComparison.InvariantCultureIgnoreCase));
        var solutionArchitectProfession = professions.FirstOrDefault(
            x => x.Title.Equals("Архитектор (Architect)", StringComparison.InvariantCultureIgnoreCase));

        if (Grade == "Teamlead/Techlead" && techleadProfession != null)
        {
            professionOrNull = techleadProfession;
        }
        else if (Grade == "Teamlead" && teamleadProfession != null)
        {
            professionOrNull = teamleadProfession;
        }
        else if (Grade == "CTO" && ctoProfession != null)
        {
            professionOrNull = ctoProfession;
        }
        else if (Grade == "SolutionArchitect" && solutionArchitectProfession != null)
        {
            professionOrNull = solutionArchitectProfession;
        }
        else if (CurrentProfession != null)
        {
            var profession = CurrentProfession.ToLowerInvariant();
            professionOrNull = professions.FirstOrDefault(x =>
                x.Title.Contains(profession, StringComparison.InvariantCultureIgnoreCase));
        }

        var salary = new UserSalary(
            null,
            SalaryNett.Value,
            4,
            2021,
            Currency.KZT,
            DeveloperGradeAsEnum,
            CompanyTypeAsEnum.Value,
            skillOrNull,
            workIndustryOrNull,
            professionOrNull,
            CityAsEnum,
            true,
            SalarySourceType.KolesaDevelopersCsv2022,
            Age,
            2021 - Experience,
            GenderAsEnum);

        return salary;
    }
}