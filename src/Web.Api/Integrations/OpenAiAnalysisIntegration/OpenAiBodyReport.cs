using System.Collections.Generic;
using Domain.Entities.Salaries;
using Domain.Entities.StatData;

namespace Web.Api.Integrations.OpenAiAnalysisIntegration;

public record OpenAiBodyReport
{
    private readonly List<long> _professions;
    private readonly Currency _currency;

    public OpenAiBodyReport(
        List<long> professions,
        List<StatDataChangeSubscriptionRecord> subscriptionStatData,
        Currency currency)
    {
        _professions = professions;
        _currency = currency;
        ReportMetadata = new OpenAiBodyReportMetadata(_currency);
    }

    public OpenAiBodyReportMetadata ReportMetadata { get; }

    public List<OpenAiBodyReportRole> Roles { get; }
}