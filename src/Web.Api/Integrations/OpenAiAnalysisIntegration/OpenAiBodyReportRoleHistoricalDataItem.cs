using System;
using System.Collections.Generic;
using System.Linq;
using Domain.Entities.StatData;

namespace Web.Api.Integrations.OpenAiAnalysisIntegration;

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
            PercentChange = 0;
            return;
        }

        Average = salariesForDate.Average(x => x.Value);
        PercentChange = Math.Round(
            (Average - averageSalaryToCompare) / averageSalaryToCompare,
            2);
    }

    public string Date { get; }

    public double Average { get; }

    public double PercentChange { get; }
}