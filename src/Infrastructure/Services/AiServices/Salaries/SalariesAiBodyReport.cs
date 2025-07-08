using Domain.Entities.Salaries;
using Infrastructure.Salaries;

namespace Infrastructure.Services.AiServices.Salaries;

public record SalariesAiBodyReport
{
    private readonly SalarySubscriptionData _subscriptionData;

    public SalariesAiBodyReport(
        SalarySubscriptionData subscriptionData,
        Currency currency)
    {
        _subscriptionData = subscriptionData.IsInitializedOrFail();
        ReportMetadata = new SalariesAiBodyReportMetadata(currency);
        Roles = new List<SalariesAiBodyReportRole>();

        var now = DateTimeOffset.UtcNow;

        foreach (var profession in subscriptionData.FilterData.SelectedProfessions)
        {
            var salariesForProfession = subscriptionData.Salaries
                .Where(x => x.ProfessionId == profession.Id)
                .ToList();

            Roles.Add(
                new SalariesAiBodyReportRole(
                    profession,
                    salariesForProfession,
                    now));
        }
    }

    public SalariesAiBodyReportMetadata ReportMetadata { get; }

    public List<SalariesAiBodyReportRole> Roles { get; }

    public string ToJson()
    {
        return System.Text.Json.JsonSerializer.Serialize(this);
    }
}