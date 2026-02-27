using Domain.Entities.Salaries;

namespace Infrastructure.Salaries;

public record SalaryInfo
{
    public SalaryInfo(
        double? minSalary,
        double? maxSalary,
        string originalText,
        bool hasHashtag,
        Currency? currency = null)
    {
        MinSalary = minSalary;
        MaxSalary = maxSalary;
        OriginalText = originalText;
        HasHashtag = hasHashtag;
        Currency = currency;
    }

    public double? MinSalary { get; }

    public double? MaxSalary { get; }

    public string OriginalText { get; }

    public bool HasHashtag { get; }

    public Currency? Currency { get; }

    public static SalaryInfo NoInfo(
        string originalText)
    {
        return new SalaryInfo(
            null,
            null,
            originalText,
            false);
    }

    public bool HasAnySalary()
    {
        if (MinSalary.HasValue && MaxSalary.HasValue)
        {
            return MaxSalary.Value >= MinSalary.Value;
        }

        return MinSalary.HasValue || MaxSalary.HasValue;
    }

    public string ToTelegramHtml()
    {
        var currencySymbol = Currency switch
        {
            Domain.Entities.Salaries.Currency.USD => "$",
            Domain.Entities.Salaries.Currency.EUR => "€",
            _ => "₸",
        };

        var stringBuilder = new System.Text.StringBuilder();
        stringBuilder.Append("Указанная зарплата в вакансии: ");

        if (MinSalary.HasValue)
        {
            stringBuilder.Append($"от <b>{MinSalary.Value:N0}</b>{currencySymbol} ");
        }

        if (MaxSalary.HasValue)
        {
            stringBuilder.Append($"до <b>{MaxSalary.Value:N0}</b>{currencySymbol}.");
        }

        return stringBuilder.ToString();
    }
}