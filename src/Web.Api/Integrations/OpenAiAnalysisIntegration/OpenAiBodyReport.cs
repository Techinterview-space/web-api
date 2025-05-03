using System;
using System.Collections.Generic;
using System.Linq;
using Domain.Entities.Salaries;
using Web.Api.Features.BackgroundJobs;

namespace Web.Api.Integrations.OpenAiAnalysisIntegration;

public record OpenAiBodyReport
{
    private readonly SalarySubscriptionData _subscriptionData;

    public OpenAiBodyReport(
        SalarySubscriptionData subscriptionData,
        Currency currency)
    {
        _subscriptionData = subscriptionData.IsInitializedOrFail();
        ReportMetadata = new OpenAiBodyReportMetadata(currency);
        Roles = new List<OpenAiBodyReportRole>();

        var now = DateTimeOffset.UtcNow;

        foreach (var profession in subscriptionData.FilterData.SelectedProfessions)
        {
            var salariesForProfession = subscriptionData.Salaries
                .Where(x => x.ProfessionId == profession.Id)
                .ToList();

            Roles.Add(
                new OpenAiBodyReportRole(
                    profession,
                    salariesForProfession,
                    now));
        }
    }

    public OpenAiBodyReportMetadata ReportMetadata { get; }

    public List<OpenAiBodyReportRole> Roles { get; }
}