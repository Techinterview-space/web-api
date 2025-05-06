using Domain.Entities.StatData;
using Domain.Extensions;

namespace Infrastructure.Services.OpenAi.Models;

public record OpenAiBodyReportRoleHistoricalDataItem
{
    public OpenAiBodyReportRoleHistoricalDataItem(
        List<SalaryBaseData> salariesForDate,
        DateTimeOffset date,
        double averageSalaryToCompare)
    {
        Date = date.ToString("yyyy-MM-dd");
        if (salariesForDate.Count == 0)
        {
            Average = 0;
            Median = 0;
            PercentChange = 0;
            Count = 0;
            return;
        }

        Count = salariesForDate.Count;
        Average = salariesForDate.Average(x => x.Value);
        Median = salariesForDate.Median(x => x.Value);

        PercentChange = Math.Round(
            (Average - averageSalaryToCompare) / averageSalaryToCompare,
            2);
    }

    public string Date { get; }

    public double Average { get; }

    public double Median { get; }

    public int Count { get; }

    public double PercentChange { get; }
}