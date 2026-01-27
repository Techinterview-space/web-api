using System.Text.Json.Serialization;
using Domain.Entities.Salaries;

namespace Infrastructure.Services.AiServices.Salaries;

public record SalariesAiBodyReportMetadata
{
    [JsonPropertyName("reportDate")]
    public string ReportDate { get; init; }

    [JsonPropertyName("currency")]
    public string Currency { get; init; }

    [JsonPropertyName("periodType")]
    public string PeriodType { get; init; }

    public SalariesAiBodyReportMetadata(
        Currency currency)
    {
        Currency = currency.ToString();
        ReportDate = DateTime.UtcNow.ToString("yyyy-MM-dd");
        PeriodType = "weekly";
    }

    public SalariesAiBodyReportMetadata()
    {
    }
}