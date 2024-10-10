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
    public string RowId { get; set; }

    public string WorkIndustry { get; set; }

    public string City { get; set; }

    public KazakhstanCity? GetCityAsEnum()
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

    public string CompanyType { get; set; }

    public CompanyType? GetCompanyTypeAsEnum()
    {
        return CompanyType.ToEnum<CompanyType>();
    }

    public string WorkMode { get; set; }

    public string Grade { get; set; }

    public DeveloperGrade? DeveloperGradeAsEnum()
    {
        return Grade.ToEnum<DeveloperGrade>();
    }

    public string Gender { get; set; }

    public Gender? GetGenderAsEnum()
    {
        return Gender.ToEnum<Gender>();
    }

    public string Profession { get; set; }

    public int? Salary { get; set; }

    [Format("dd.MM.yyyy HH:mm:ss")]
    [Optional]
    public DateTime? AnswerDate { get; set; }

    public UserSalary CreateUserSalary(
        List<Skill> skills,
        List<WorkIndustry> workIndustries,
        List<Profession> professions)
    {
        if (Salary == null ||
            GetCompanyTypeAsEnum() == null)
        {
            throw new InvalidOperationException("Salary record is not valid");
        }

        WorkIndustry workIndustryOrNull = null;
        Profession professionOrNull = null;

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
        else if (Profession != null)
        {
            var profession = Profession.ToLowerInvariant();
            professionOrNull = professions.FirstOrDefault(x =>
                x.Title.Contains(profession, StringComparison.InvariantCultureIgnoreCase));
        }

        var salary = new UserSalary(
            null,
            Salary.Value,
            4,
            2024,
            Currency.KZT,
            DeveloperGradeAsEnum(),
            GetCompanyTypeAsEnum().GetValueOrDefault(Domain.Entities.Salaries.CompanyType.Local),
            null,
            workIndustryOrNull,
            professionOrNull,
            GetCityAsEnum(),
            true,
            SalarySourceType.KolesaDataAnalystCsv2024,
            null,
            null,
            GetGenderAsEnum());

        return salary;
    }
}