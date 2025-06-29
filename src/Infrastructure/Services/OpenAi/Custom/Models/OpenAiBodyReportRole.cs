using Domain.Entities.Salaries;
using Domain.Entities.StatData;

namespace Infrastructure.Services.OpenAi.Custom.Models;

public record OpenAiBodyReportRole
{
    private const int HistoricalDataCount = 3;

    public OpenAiBodyReportRole(
        Profession profession,
        List<SalaryBaseData> salaries,
        DateTimeOffset now)
    {
        RoleName = profession.Title;
        CurrentSalary = new OpenAiBodyReportRoleSalaryData(salaries);
        HistoricalData = new List<OpenAiBodyReportRoleHistoricalDataItem>();

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
                new OpenAiBodyReportRoleHistoricalDataItem(
                    salariesForDate,
                    now.AddDays(-daysCount),
                    averageSalaryToCompare));
        }
    }

    public string RoleName { get; }

    public OpenAiBodyReportRoleSalaryData CurrentSalary { get; }

    public List<OpenAiBodyReportRoleHistoricalDataItem> HistoricalData { get; }
}