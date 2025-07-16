using Domain.Entities.Salaries;
using Domain.ValueObjects;
using Infrastructure.Salaries;

namespace Infrastructure.Services.AiServices.Salaries;

public record SalariesAiBodyReport
{
    private readonly SalarySubscriptionData _subscriptionData;

    public SalariesAiBodyReport()
    {
    }

    public SalariesAiBodyReport(
        SalarySubscriptionData subscriptionData,
        Currency currency)
    {
        _subscriptionData = subscriptionData.IsInitializedOrFail();
        ReportMetadata = new SalariesAiBodyReportMetadata(currency);
        Roles = new List<SalariesAiBodyReportRole>();

        var now = DateTimeOffset.UtcNow;

        foreach (var profession in subscriptionData.FilterData.SelectedProfessions)
        {
            var salariesForProfession = subscriptionData.Salaries
                .Where(x => x.ProfessionId == profession.Id)
                .ToList();

            Roles.Add(
                new SalariesAiBodyReportRole(
                    profession,
                    salariesForProfession,
                    now));
        }
    }

    public SalariesAiBodyReportMetadata ReportMetadata { get; }

    public List<SalariesAiBodyReportRole> Roles { get; }

    public string ToJson()
    {
        return System.Text.Json.JsonSerializer.Serialize(this);
    }

    public string ToTelegramHtmlSummary()
    {
        var stringBuilder = new System.Text.StringBuilder();
        stringBuilder.Append("Рейтинг медианных зарплат (от высокой к низкой)\n\n");

        var counter = 1;
        foreach (var role in Roles.OrderByDescending(x => x.CurrentSalary.Median))
        {
            stringBuilder.AppendLine($"{counter}. <b>{role.RoleName}</b> - {new SalaryShortFormattedValue(role.CurrentSalary.Median)}");
            stringBuilder.AppendLine();

            counter++;
        }

        foreach (var role in Roles)
        {
            stringBuilder.AppendLine($"<b>{role.RoleName}</b>");

            if (role.HistoricalData.Count == 0)
            {
                stringBuilder.AppendLine($"{role.CurrentSalary.Count}шт, изменений медианы нет");
                continue;
            }

            var medians = role.HistoricalData
                .Select(x => x.Median)
                .ToList();

            medians.Add(role.CurrentSalary.Median);

            var isStable = medians
                .All(x => x >= x * 0.98 && x <= x * 1.02);

            var firstHistoricalData = role.HistoricalData[^1];
            var changeInPercent = (role.CurrentSalary.Median - firstHistoricalData.Median) / firstHistoricalData.Median * 100;
            var countOfSalaries = role.CurrentSalary.Count - firstHistoricalData.Count;

            var textForCountChange = countOfSalaries > 0
                ? $" (+{countOfSalaries})"
                : string.Empty;

            if (isStable || role.HistoricalData.Count == 1)
            {
                stringBuilder.AppendLine($"{role.CurrentSalary.Count}шт{textForCountChange}, изменений медианы нет");
                continue;
            }

            if (changeInPercent is >= -1 and <= 1)
            {
                stringBuilder.AppendLine($"{role.CurrentSalary.Count}шт{textForCountChange}, колебание медианы в рамках 1 процента");
                continue;
            }

            var now = DateTime.UtcNow;
            stringBuilder.AppendLine(
                $"{new SalaryShortFormattedValue(firstHistoricalData.Median)} ({firstHistoricalData.Date}) -> " +
                $"{new SalaryShortFormattedValue(role.CurrentSalary.Median)} ({now:dd.MM}). " +
                $"Изменение на {changeInPercent:F2}%");

            stringBuilder.AppendLine($"{role.CurrentSalary.Count}шт{textForCountChange}");
        }

        return stringBuilder.ToString();
    }
}