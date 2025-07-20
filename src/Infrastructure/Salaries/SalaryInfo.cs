namespace Infrastructure.Salaries;

public record SalaryInfo
{
    public SalaryInfo(
        double? minSalary,
        double? maxSalary,
        string originalText,
        bool hasHashtag)
    {
        MinSalary = minSalary;
        MaxSalary = maxSalary;
        OriginalText = originalText;
        HasHashtag = hasHashtag;
    }

    public double? MinSalary { get; }

    public double? MaxSalary { get; }

    public string OriginalText { get; }

    public bool HasHashtag { get; }

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
        => MinSalary.HasValue || MaxSalary.HasValue;

    public string ToTelegramHtml()
    {
        var stringBuilder = new System.Text.StringBuilder();
        stringBuilder.Append("Указанная зарплата в вакансии: ");

        if (MinSalary.HasValue)
        {
            stringBuilder.Append($"от <b>{MinSalary.Value:N0}</b> ");
        }

        if (MaxSalary.HasValue)
        {
            stringBuilder.Append($"до <b>{MaxSalary.Value:N0}</b>.");
        }

        return stringBuilder.ToString();
    }
}