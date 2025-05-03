using System;
using Domain.Entities.Salaries;

namespace Web.Api.Integrations.OpenAiAnalysisIntegration;

public record OpenAiBodyReportMetadata
{
    public string ReportDate { get; }

    public string Currency { get; }

    public string PeriodType { get; }

    public OpenAiBodyReportMetadata(
        Currency currency)
    {
        Currency = currency.ToString();
        ReportDate = DateTime.UtcNow.ToString("yyyy-MM-dd");
        PeriodType = "weekly";
    }
}