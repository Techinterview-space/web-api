namespace Web.Api.Integrations.OpenAiAnalysisIntegration;

public record OpenAiBodyReportRoleSalaryData
{
    public OpenAiBodyReportRoleSalaryData(
        double average,
        double median,
        double min,
        double max,
        int count)
    {
        Average = average;
        Median = median;
        Min = min;
        Max = max;
        Count = count;
    }

    public double Average { get; }

    public double Median { get; }

    public double Min { get; }

    public double Max { get; }

    public int Count { get; }
}