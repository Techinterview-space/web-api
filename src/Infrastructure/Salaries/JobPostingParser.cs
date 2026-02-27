using System.Globalization;
using System.Text.RegularExpressions;
using Domain.Entities.Salaries;

namespace Infrastructure.Salaries;

public class JobPostingParser
{
    public const int MinimalSalaryThreshold = 85_000;

    private static readonly Regex SalaryWithTengeBeforeRegex = new Regex(
        @"до \b\d{1,3}(?: \d{3})*\s*тенге\b",
        RegexOptions.IgnoreCase);

    private static readonly Regex SalaryWithTengeFromRegex = new Regex(
        @"от \b\d{1,3}(?: \d{3})*\s*тенге\b",
        RegexOptions.IgnoreCase);

    // Regex pattern for detecting job postings with salary ranges
    private static readonly Regex JobPostingThousandsRegex = new Regex(
        @"#вакансия.*?(?:вилка|зарплата|зп|от|до|salary)\s*(?:от\s*)?(\d+(?:\s*\d+)*)\s*(?:к|000|тыс|тысяч)?\s*(?:-|–|—|до)\s*(\d+(?:\s*\d+)*)\s*(?:к|000|тыс|тысяч)?|" +
        @"#вакансия.*?(?:вилка|зарплата|зп|от|salary)\s*(?:от\s*)?(\d+(?:\s*\d+)*)\s*(?:к|000|тыс|тысяч)",
        RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.Singleline);

    // More specific regex patterns for different salary formats
    private static readonly Regex SalaryRangeInThousandsRegex = new Regex(
        @"(?:вилка|зарплата|зп|от|до|salary|заработная плата)?\s*:?\s*(?:от\s*)?((?:\d+\s*)+\d+)\s*(?:к|000|тыс|тысяч|₸|\$|USD|EUR|€)?\s*(?:-|–|—|до)\s*((?:\d+\s*)+\d+)\s*(?:к|000|тыс|тысяч|тенге|тг|₸|\$|USD|EUR|€)?",
        RegexOptions.IgnoreCase);

    private static readonly Regex FromSalarySingleThousandRegex = new Regex(
        @"(?:вилка|зарплата|зп|от|salary|заработная плата)\s*:?\s*(?:от\s*)?((?:\d+\s*)+\d+)\s*(?:к|000|тыс|тысяч|тг|тенге|₸|\$|USD|EUR|€)",
        RegexOptions.IgnoreCase);

    private static readonly Regex UpToSalarySingleThousandRegex = new Regex(
        @"(?:вилка|зарплата|зп|до|salary|заработная плата)\s*:?\s*(?:до\s*)?((?:\d+\s*)+\d+)\s*(?:к|000|тыс|тысяч|тг|тенге|₸|\$|USD|EUR|€)",
        RegexOptions.IgnoreCase);

    // Additional regex for million format like "От 1.2млн до 1.9млн"
    private static readonly Regex SalaryRangeInMillionRegex = new Regex(
        @"(?:от\s*)?(\d+(?:[.,]\d+)?)\s*(?:млн|million|м)\s*(?:до|-)?\s*(\d+(?:[.,]\d+)?)\s*(?:млн|million|м)",
        RegexOptions.IgnoreCase);

    private static readonly Regex FromSalarySingleMillionRegex = new Regex(
        @"((?:от\s*)|(?:>|>=\s*))(\d+(?:[.,]\d+)?)\s*(?:млн|million|м)(?!\s*(?:до|-))",
        RegexOptions.IgnoreCase);

    private static readonly Regex UpToSalarySingleMillionRegex = new Regex(
        @"((?:до\s*)|(?:<=|<\s*))(\d+(?:[.,]\d+)?)\s*(?:млн|million|м)(?!\s*(?:до|-))",
        RegexOptions.IgnoreCase);

    private static readonly Regex SalariesWithDotsRegex = new Regex(
        @"(\d+(?:\.\d{3})+)",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static readonly Regex JustOneSalaryValueRegex = new Regex(
        @"\b\d{1,3}(?:(?:\.|,|'|\s)\d{3})+\b",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private readonly string _sourceMessage;

    public JobPostingParser(
        string sourceMessage)
    {
        _sourceMessage = sourceMessage?
            .Replace("на руки", string.Empty)
            .Replace("8:00", string.Empty)
            .Replace("9:00", string.Empty)
            .Replace("10:00", string.Empty)
            .Replace("17:00", string.Empty)
            .Replace("18:00", string.Empty)
            .Replace("19:00", string.Empty) ?? string.Empty;

        _sourceMessage = SalariesWithDotsRegex.Replace(
            _sourceMessage, m => m.Value.Replace(".", " "));
    }

    public SalaryInfo GetResult()
    {
        // Check if message contains #вакансия
        if (string.IsNullOrEmpty(_sourceMessage) ||
            !_sourceMessage.Contains("#вакансия", StringComparison.InvariantCultureIgnoreCase))
        {
            return SalaryInfo.NoInfo(_sourceMessage);
        }

        var currency = DetectCurrency();
        var isKzt = currency is null or Currency.KZT;

        double? minSalary = null;
        double? maxSalary = null;
        string originalText = null;

        // Try to find a million salary range first (most specific)
        var millionRangeMatch = SalaryRangeInMillionRegex.Match(_sourceMessage);
        if (millionRangeMatch.Success)
        {
            minSalary = ParseSalaryInMillions(millionRangeMatch.Groups[1].Value);
            maxSalary = ParseSalaryInMillions(millionRangeMatch.Groups[2].Value);
            originalText = millionRangeMatch.Value;

            return new SalaryInfo(
                minSalary,
                maxSalary,
                originalText,
                true,
                currency);
        }

        // Try to find single million salary value
        var millionSingleMatch = FromSalarySingleMillionRegex.Match(_sourceMessage);
        if (millionSingleMatch.Success)
        {
            minSalary = ParseSalaryInMillions(millionSingleMatch.Groups[2].Value);
            originalText = millionSingleMatch.Value;

            return new SalaryInfo(
                minSalary,
                null,
                originalText,
                true,
                currency);
        }

        var upToMillionSingleMatch = UpToSalarySingleMillionRegex.Match(_sourceMessage);
        if (upToMillionSingleMatch.Success)
        {
            maxSalary = ParseSalaryInMillions(upToMillionSingleMatch.Groups[2].Value);
            originalText = upToMillionSingleMatch.Value;

            return new SalaryInfo(
                null,
                maxSalary,
                originalText,
                true,
                currency);
        }

        // Try to find salary range first
        var rangeMatch = SalaryRangeInThousandsRegex.Match(_sourceMessage);
        if (rangeMatch.Success)
        {
            var fullMatch = rangeMatch.Groups[0].Value;
            var containsThousands = fullMatch.Contains("тыс") ||
                                    fullMatch.Contains("к") ||
                                    fullMatch.Contains("k");

            minSalary = ParseSalaryInThousands(rangeMatch.Groups[1].Value, containsThousands && isKzt);
            maxSalary = ParseSalaryInThousands(rangeMatch.Groups[2].Value, containsThousands && isKzt);
            originalText = rangeMatch.Value;

            var meetsThreshold = isKzt
                ? minSalary >= MinimalSalaryThreshold && maxSalary >= MinimalSalaryThreshold
                : minSalary > 0 && maxSalary > 0;

            if (meetsThreshold && ((maxSalary - minSalary) / minSalary >= 0.1))
            {
                return new SalaryInfo(
                    minSalary,
                    maxSalary,
                    originalText,
                    true,
                    currency);
            }
        }

        // Try to find single salary value
        var fromSingleMatch = FromSalarySingleThousandRegex.Match(_sourceMessage);
        if (fromSingleMatch.Success)
        {
            minSalary = ParseSalaryInThousands(fromSingleMatch.Groups[1].Value, isKzt);
            originalText = fromSingleMatch.Value;

            return new SalaryInfo(
                minSalary,
                null,
                originalText,
                true,
                currency);
        }

        var upToSingleSalary = UpToSalarySingleThousandRegex.Match(_sourceMessage);
        if (upToSingleSalary.Success)
        {
            maxSalary = ParseSalaryInThousands(upToSingleSalary.Groups[1].Value, isKzt);
            originalText = upToSingleSalary.Value;

            return new SalaryInfo(
                null,
                maxSalary,
                originalText,
                true,
                currency);
        }

        var justOneSalary = JustOneSalaryValueRegex.Match(_sourceMessage);
        if (justOneSalary.Success)
        {
            minSalary = ParseSalaryInThousands(justOneSalary.Groups[1].Value);
            if (minSalary == 0)
            {
                minSalary = ParseSalaryInThousands(justOneSalary.Groups[0].Value);
            }

            originalText = justOneSalary.Value;

            return new SalaryInfo(
                minSalary,
                null,
                originalText,
                true,
                currency);
        }

        var tengeMatch = SalaryWithTengeBeforeRegex.Match(_sourceMessage);
        if (tengeMatch.Success &&
            double.TryParse(tengeMatch.Groups[0].Value.Replace("тенге", string.Empty).Trim(), out var tengeValue))
        {
            originalText = tengeMatch.Groups[0].Value.Replace("тенге", string.Empty).Trim();
            minSalary = tengeValue;
            return new SalaryInfo(
                minSalary,
                null,
                originalText,
                true,
                currency);
        }

        return new SalaryInfo(
            null,
            null,
            _sourceMessage,
            true,
            currency);
    }

    private Currency? DetectCurrency()
    {
        if (_sourceMessage.Contains("тенге", StringComparison.InvariantCultureIgnoreCase) ||
            _sourceMessage.Contains("тг", StringComparison.InvariantCultureIgnoreCase) ||
            _sourceMessage.Contains("₸") ||
            _sourceMessage.Contains("KZT", StringComparison.InvariantCultureIgnoreCase))
        {
            return Currency.KZT;
        }

        if (_sourceMessage.Contains('$') ||
            _sourceMessage.Contains("USD", StringComparison.InvariantCultureIgnoreCase))
        {
            return Currency.USD;
        }

        if (_sourceMessage.Contains('€') ||
            _sourceMessage.Contains("EUR", StringComparison.InvariantCultureIgnoreCase))
        {
            return Currency.EUR;
        }

        return null;
    }

    private static double ParseSalaryInThousands(
        string salaryText,
        bool adjustThousandsLessThan10k = true)
    {
        // Remove spaces and parse the number
        var cleanText = salaryText.Replace(" ", string.Empty);

        if (double.TryParse(cleanText, out var value))
        {
            // Check if it's in thousands (к, тыс, тысяч)
            // If the number is less than 10000, assume it's in thousands
            if (value < 10000 && adjustThousandsLessThan10k)
            {
                return value * 1000;
            }

            return value;
        }

        return 0;
    }

    private static double ParseSalaryInMillions(
        string salaryText)
    {
        // Replace comma with dot for decimal parsing and remove spaces
        var cleanText = salaryText
            .Replace(",", ".")
            .Replace(" ", string.Empty)
            .Trim();

        if (double.TryParse(cleanText, NumberStyles.Any, CultureInfo.InvariantCulture, out var value))
        {
            // Convert millions to actual value
            return value * 1_000_000;
        }

        return 0;
    }

    public static bool IsJobPostingWithSalary(string message)
    {
        return JobPostingThousandsRegex.IsMatch(message);
    }
}