using System;
using System.Collections.Generic;
using System.Linq;
using Domain.Entities.Enums;
using Domain.Entities.Salaries;
using Domain.Enums;
using Domain.Extensions;

namespace Web.Api.Features.Import.ImportKolesaCsv;

public record KolesaDeveloperCsvLine
{
    public string RowId { get; set; }

    public string WorkIndustry { get; set; }

    public string CompanyType { get; set; }

    public string Grade { get; set; }

    public string Profession { get; set; }

    public string Salary { get; set; }

    public string City { get; set; }

    public string Gender { get; set; }

    public Gender? GetGenderAsEnum()
    {
        if (string.IsNullOrEmpty(Gender))
        {
            return null;
        }

        return Gender switch
        {
            "мужской" => Domain.Enums.Gender.Male,
            "женский" => Domain.Enums.Gender.Female,
            _ => Gender.ToEnum<Gender>()
        };
    }

    public DeveloperGrade? DeveloperGradeAsEnum()
    {
        return Grade switch
        {
            "специалист среднего звена: средние задачи делаю сам и сложные — под присмотром" => DeveloperGrade.Middle,
            "старший специалист: сам делаю проект любой сложности, состоящий из одной и более задач. Консультирую стажеров и спецов среднего уровня"
                => DeveloperGrade.Senior,
            "джун: простые задачи делаю сам, средние – под присмотром" => DeveloperGrade.Junior,
            _ => Grade.ToEnum<DeveloperGrade>()
        };
    }

    public CompanyType? GetCompanyTypeAsEnum()
    {
        return CompanyType.ToEnum<CompanyType>();
    }

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
            var workIndustry = WorkIndustry switch
            {
                "телеком-компания" => "телеком",
                "финансы, банковское дело" => "банк",
                "outsource IT-компания" => "it аутсорс/аутстафф",
                _ => WorkIndustry.ToLowerInvariant()
            };

            workIndustryOrNull = workIndustries.FirstOrDefault(x =>
                x.Title.Contains(workIndustry, StringComparison.InvariantCultureIgnoreCase));
        }

        var teamleadProfession = professions.FirstOrDefault(
            x => x.Title.Contains("Teamleader", StringComparison.InvariantCultureIgnoreCase));

        var headOfDepartmentProfession = professions.FirstOrDefault(
            x => x.Title.Contains("Head of department", StringComparison.InvariantCultureIgnoreCase));

        if (Profession != null)
        {
            var profession = Profession switch
            {
                "Администратор базы данных" => "Администратор Баз Данных (Database Administrator)".ToLowerInvariant(),
                "Software engineer" => "developer",
                "Solution architect" => "Архитектор (Architect)",
                "Дата аналитик" => "data analyst",
                "ML developer" => "Machine Learning Developer (ML)".ToLowerInvariant(),
                _ => Profession.ToLowerInvariant(),
            };

            professionOrNull = professions.FirstOrDefault(x =>
                x.Title.Contains(profession, StringComparison.InvariantCultureIgnoreCase));
        }

        DeveloperGrade? grade = null;
        Skill skillOrNull = null;

        if (Grade != null)
        {
            if (string.Equals(Grade, "teamlead", StringComparison.InvariantCultureIgnoreCase))
            {
                if (Profession != null)
                {
                    var skillTitle = Profession switch
                    {
                        "Data analyst" => "data analysis",
                        _ => Profession
                    };

                    skillOrNull = skills.FirstOrDefault(x =>
                        x.Title.Contains(skillTitle, StringComparison.InvariantCultureIgnoreCase));
                }

                grade = DeveloperGrade.Lead;
                professionOrNull = teamleadProfession;
            }

            if (string.Equals(Grade, "head of department", StringComparison.InvariantCultureIgnoreCase))
            {
                if (Profession != null)
                {
                    var skillTitle = Profession switch
                    {
                        "Data analyst" => "data analysis",
                        _ => Profession
                    };

                    skillOrNull = skills.FirstOrDefault(x =>
                        x.Title.Contains(skillTitle, StringComparison.InvariantCultureIgnoreCase));
                }

                grade = DeveloperGrade.Lead;
                professionOrNull = headOfDepartmentProfession;
            }

            grade ??= DeveloperGradeAsEnum();
        }

        int? salaryValue = null;
        if (Salary != null)
        {
            var cleanedSalary = Salary
                .Replace(" ", string.Empty)
                .Replace("тг", string.Empty);

            if (!int.TryParse(cleanedSalary, out var value))
            {
                return null;
            }

            salaryValue = value;
        }

        if (salaryValue == null)
        {
            return null;
        }

        var salary = new UserSalary(
            null,
            salaryValue.Value,
            4,
            2024,
            Currency.KZT,
            grade,
            GetCompanyTypeAsEnum().GetValueOrDefault(Domain.Entities.Salaries.CompanyType.Local),
            skillOrNull,
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