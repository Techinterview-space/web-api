using Domain.Entities.Salaries;
using Infrastructure.Salaries;

namespace Infrastructure.Services.OpenAi.Models;

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

    public string ToJson()
    {
        return System.Text.Json.JsonSerializer.Serialize(this);
    }
}