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
}