using System.Text;
using Infrastructure.Salaries;
using Web.Api.Features.Salaries.Models;

namespace Web.Api.Features.Salaries.ExportCsv;

public record SalariesCsvStringBuilder
{
    private const string NoValue = "";

    private readonly SelectBoxItemsResponse _selectBoxItemsResponse;
    private readonly StringBuilder _stringBuilder;

    public SalariesCsvStringBuilder(
        SelectBoxItemsResponse selectBoxItemsResponse)
    {
        _selectBoxItemsResponse = selectBoxItemsResponse;
        _stringBuilder = new StringBuilder();
    }

    public SalariesCsvStringBuilder AppendHeader()
    {
        _stringBuilder.AppendLine("Value,Quarter,Year,Age,Gender,Started,Profession,Grade,Company,City,Skill,Industry,Created");
        return this;
    }

    public SalariesCsvStringBuilder AppendSalary(
        UserSalaryDto salary)
    {
        var profession = _selectBoxItemsResponse.GetProfessionOrNull(salary.ProfessionId);
        var skill = _selectBoxItemsResponse.GetSkillOrNull(salary.SkillId);
        var workIndustry = _selectBoxItemsResponse.GetIndustryOrNull(salary.WorkIndustryId);

        _stringBuilder.AppendLine(
            $"{salary.Value}," +
            $"{salary.Quarter}," +
            $"{salary.Year}," +
            $"{salary.Age?.ToString() ?? NoValue}," +
            $"{salary.Gender?.ToString() ?? NoValue}," +
            $"{salary.YearOfStartingWork?.ToString() ?? NoValue}," +
            $"{profession?.Title ?? NoValue}," +
            $"{salary.Grade?.ToString() ?? NoValue}," +
            $"{salary.Company.ToString()}," +
            $"{salary.City?.ToString() ?? NoValue}," +
            $"{skill?.Title ?? NoValue}," +
            $"{workIndustry?.Title ?? NoValue}," +
            $"{salary.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss") ?? NoValue}");

        return this;
    }

    public override string ToString()
    {
        return _stringBuilder.ToString();
    }
}