using System.Text.Json.Serialization;
using Domain.Entities.StatData.Salary;
using Domain.Extensions;

namespace Infrastructure.Services.AiServices.Salaries;

public record SalariesAiBodyReportRoleHistoricalDataItem
{
    public SalariesAiBodyReportRoleHistoricalDataItem(
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

    public SalariesAiBodyReportRoleHistoricalDataItem()
    {
    }

    [JsonPropertyName("date")]
    public string Date { get; init; }

    [JsonPropertyName("average")]
    public double Average { get; init; }

    [JsonPropertyName("median")]
    public double Median { get; init; }

    [JsonPropertyName("count")]
    public int Count { get; init; }

    [JsonPropertyName("percentChange")]
    public double PercentChange { get; init; }
}