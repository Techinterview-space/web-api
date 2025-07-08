using Domain.Entities.Salaries;

namespace Infrastructure.Services.AiServices.Salaries;

public record SalariesAiBodyReportMetadata
{
    public string ReportDate { get; }

    public string Currency { get; }

    public string PeriodType { get; }

    public SalariesAiBodyReportMetadata(
        Currency currency)
    {
        Currency = currency.ToString();
        ReportDate = DateTime.UtcNow.ToString("yyyy-MM-dd");
        PeriodType = "weekly";
    }
}