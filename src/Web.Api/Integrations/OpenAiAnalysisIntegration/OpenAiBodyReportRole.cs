namespace Web.Api.Integrations.OpenAiAnalysisIntegration;

public record OpenAiBodyReportRole
{
    public string RoleName { get; }

    public OpenAiBodyReportRoleSalaryData CurrentSalary { get; }

    public OpenAiBodyReportRoleSalaryData HistoricalData { get; }
}