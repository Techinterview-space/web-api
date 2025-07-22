using System.Globalization;
using System.Text.RegularExpressions;

namespace Infrastructure.Salaries;

public class JobPostingParser
{
    // Regex pattern for detecting job postings with salary ranges
    private static readonly Regex JobPostingThousandsRegex = new Regex(
        @"#вакансия.*?(?:вилка|зарплата|зп|от|до|salary)\s*(?:от\s*)?(\d+(?:\s*\d+)*)\s*(?:к|000|тыс|тысяч)?\s*(?:-|–|—|до)\s*(\d+(?:\s*\d+)*)\s*(?:к|000|тыс|тысяч)?|" +
        @"#вакансия.*?(?:вилка|зарплата|зп|от|salary)\s*(?:от\s*)?(\d+(?:\s*\d+)*)\s*(?:к|000|тыс|тысяч)",
        RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.Singleline);

    // More specific regex patterns for different salary formats
    private static readonly Regex SalaryRangeInThousandsRegex = new Regex(
        @"(?:вилка|зарплата|зп|от|до|salary)\s*:?\s*(?:от\s*)?((?:\d+\s*)+\d+)\s*(?:к|000|тыс|тысяч)?\s*(?:-|–|—|до)\s*((?:\d+\s*)+\d+)\s*(?:к|000|тыс|тысяч|тенге)?",
        RegexOptions.IgnoreCase);

    private static readonly Regex FromSalarySingleThousandRegex = new Regex(
        @"(?:вилка|зарплата|зп|от|salary)\s*:?\s*(?:от\s*)?((?:\d+\s*)+\d+)\s*(?:к|000|тыс|тысяч)",
        RegexOptions.IgnoreCase);

    private static readonly Regex UpToSalarySingleThousandRegex = new Regex(
        @"(?:вилка|зарплата|зп|до|salary)\s*:?\s*(?:до\s*)?((?:\d+\s*)+\d+)\s*(?:к|000|тыс|тысяч)",
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

    private readonly string _sourceMessage;

    public JobPostingParser(
        string sourceMessage)
    {
        _sourceMessage = sourceMessage?
            .Replace("на руки", string.Empty) ?? string.Empty;

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

        double? minSalary = null;
        double? maxSalary = null;
        string originalText = null;

        // Try to find million salary range first (most specific)
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
                true);
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
                true);
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
                true);
        }

        // Try to find salary range first
        var rangeMatch = SalaryRangeInThousandsRegex.Match(_sourceMessage);
        if (rangeMatch.Success)
        {
            minSalary = ParseSalaryInThousands(rangeMatch.Groups[1].Value);
            maxSalary = ParseSalaryInThousands(rangeMatch.Groups[2].Value);
            originalText = rangeMatch.Value;

            return new SalaryInfo(
                minSalary,
                maxSalary,
                originalText,
                true);
        }

        // Try to find single salary value
        var fromSingleMatch = FromSalarySingleThousandRegex.Match(_sourceMessage);
        if (fromSingleMatch.Success)
        {
            minSalary = ParseSalaryInThousands(fromSingleMatch.Groups[1].Value);
            originalText = fromSingleMatch.Value;

            return new SalaryInfo(
                minSalary,
                null,
                originalText,
                true);
        }

        var upToSingleSalary = UpToSalarySingleThousandRegex.Match(_sourceMessage);
        if (upToSingleSalary.Success)
        {
            maxSalary = ParseSalaryInThousands(upToSingleSalary.Groups[1].Value);
            originalText = upToSingleSalary.Value;

            return new SalaryInfo(
                null,
                maxSalary,
                originalText,
                true);
        }

        return new SalaryInfo(
            null,
            null,
            _sourceMessage,
            true);
    }

    private static double ParseSalaryInThousands(
        string salaryText)
    {
        // Remove spaces and parse the number
        var cleanText = salaryText.Replace(" ", string.Empty);

        if (double.TryParse(cleanText, out var value))
        {
            // Check if it's in thousands (к, тыс, тысяч)
            // If the number is less than 10000, assume it's in thousands
            if (value < 10000)
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