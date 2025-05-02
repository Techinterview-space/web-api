namespace Web.Api.Integrations.OpenAiAnalysisIntegration;

public record OpenAiBodyReportRoleHistoricalDataItem
{
    public string Date { get; }

    public double Average { get; }

    public double PercentChange { get; }
}