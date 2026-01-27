using System.Text.Json.Serialization;
using Domain.Entities.Salaries;
using Domain.Entities.StatData.Salary;

namespace Infrastructure.Services.AiServices.Salaries;

public record SalariesAiBodyReportRole
{
    private const int HistoricalDataCount = 3;

    public SalariesAiBodyReportRole(
        Profession profession,
        List<SalaryBaseData> salaries,
        DateTimeOffset now)
    {
        RoleName = profession.Title;
        CurrentSalary = new SalariesAiBodyReportRoleSalaryData(salaries);
        HistoricalData = new List<SalariesAiBodyReportRoleHistoricalDataItem>();

        for (var i = 0; i < HistoricalDataCount; i++)
        {
            var daysCount = (i + 1) * 7;
            var salariesForDate = salaries
                .Where(x => x.CreatedAt <= now.AddDays(-daysCount))
                .ToList();

            if (salariesForDate.Count == 0)
            {
                break;
            }

            var averageSalaryToCompare = i == 0
                ? CurrentSalary.Average
                : HistoricalData[i - 1].Average;

            HistoricalData.Add(
                new SalariesAiBodyReportRoleHistoricalDataItem(
                    salariesForDate,
                    now.AddDays(-daysCount),
                    averageSalaryToCompare));
        }
    }

    [JsonPropertyName("roleName")]
    public string RoleName { get; init; }

    [JsonPropertyName("currentSalary")]
    public SalariesAiBodyReportRoleSalaryData CurrentSalary { get; init; }

    [JsonPropertyName("historicalData")]
    public List<SalariesAiBodyReportRoleHistoricalDataItem> HistoricalData { get; init; }

    public SalariesAiBodyReportRole()
    {
    }
}