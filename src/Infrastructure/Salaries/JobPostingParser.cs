using System.Text.RegularExpressions;

namespace Infrastructure.Salaries;

public class JobPostingParser
{
    // Regex pattern for detecting job postings with salary ranges
    private static readonly Regex JobPostingRegex = new Regex(
        @"#вакансия.*?(?:вилка|зарплата|зп|от|до|salary)\s*(?:от\s*)?(\d+(?:\s*\d+)*)\s*(?:к|000|тыс|тысяч)?\s*(?:-|–|—|до)\s*(\d+(?:\s*\d+)*)\s*(?:к|000|тыс|тысяч)?|" +
        @"#вакансия.*?(?:вилка|зарплата|зп|от|salary)\s*(?:от\s*)?(\d+(?:\s*\d+)*)\s*(?:к|000|тыс|тысяч)",
        RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.Singleline);

    // More specific regex patterns for different salary formats
    private static readonly Regex SalaryRangeRegex = new Regex(
        @"(?:вилка|зарплата|зп|от|до|salary)\s*(?:от\s*)?(\d+(?:\s*\d+)*)\s*(?:к|000|тыс|тысяч)?\s*(?:-|–|—|до)\s*(\d+(?:\s*\d+)*)\s*(?:к|000|тыс|тысяч)?",
        RegexOptions.IgnoreCase);

    private static readonly Regex SalarySingleRegex = new Regex(
        @"(?:вилка|зарплата|зп|от|salary)\s*(?:от\s*)?(\d+(?:\s*\d+)*)\s*(?:к|000|тыс|тысяч)",
        RegexOptions.IgnoreCase);

    private readonly string _sourceMessage;

    public JobPostingParser(
        string sourceMessage)
    {
        _sourceMessage = sourceMessage ?? string.Empty;
    }

    public SalaryInfo GetResult()
    {
        // Check if message contains #вакансия
        if (string.IsNullOrEmpty(_sourceMessage) ||
            !_sourceMessage.Contains("#вакансия", StringComparison.InvariantCultureIgnoreCase) ||
            !JobPostingRegex.IsMatch(_sourceMessage))
        {
            return SalaryInfo.NoInfo(_sourceMessage);
        }

        double? minSalary = null;
        string originalText = null;

        // Try to find salary range first
        var rangeMatch = SalaryRangeRegex.Match(_sourceMessage);
        if (rangeMatch.Success)
        {
            minSalary = ParseSalaryValue(rangeMatch.Groups[1].Value);
            var maxSalary = ParseSalaryValue(rangeMatch.Groups[2].Value);
            originalText = rangeMatch.Value;

            return new SalaryInfo(
                minSalary,
                maxSalary,
                originalText,
                true);
        }

        // Try to find single salary value
        var singleMatch = SalarySingleRegex.Match(_sourceMessage);
        if (singleMatch.Success)
        {
            minSalary = ParseSalaryValue(singleMatch.Groups[1].Value);
            originalText = singleMatch.Value;

            return new SalaryInfo(
                minSalary,
                null,
                originalText,
                true);
        }

        return new SalaryInfo(
            null,
            null,
            _sourceMessage,
            true);
    }

    private static double ParseSalaryValue(
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

    public static bool IsJobPostingWithSalary(string message)
    {
        return JobPostingRegex.IsMatch(message);
    }
}