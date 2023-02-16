using System;
using System.Globalization;
using System.Text.RegularExpressions;

namespace Domain.Validation;

public class EmailValidRegex : Regex
{
    private const string Pattern =
        @"^(?("")("".+?(?<!\\)""@)|(([0-9a-z]((\.(?!\.))|[-!#\$%&'\*\+/=\?\^`\{\}\|~\w])*)(?<=[0-9a-z])@))" +
        @"(?(\[)(\[(\d{1,3}\.){3}\d{1,3}\])|(([0-9a-z][-0-9a-z]*[0-9a-z]*\.)+[a-z0-9][\-a-z0-9]{0,22}[a-z0-9]))$";

    public EmailValidRegex()
        : base(
            pattern: Pattern,
            options: RegexOptions.Compiled | RegexOptions.IgnoreCase,
            matchTimeout: TimeSpan.FromMilliseconds(250))
    {
    }

    public bool IsValid(string source)
    {
        if (string.IsNullOrWhiteSpace(source))
        {
            return false;
        }

        if (!TryNormalizeEmail(source, out source))
        {
            return false;
        }

        try
        {
            return IsMatch(source);
        }
        catch (RegexMatchTimeoutException)
        {
            return false;
        }
    }

    private static bool TryNormalizeEmail(string email, out string result)
    {
        result = null;
        try
        {
            // Normalize the domain
            result = Replace(
                input: email,
                pattern: @"(@)(.+)$",
                evaluator: DomainMapper,
                options: RegexOptions.None,
                matchTimeout: TimeSpan.FromMilliseconds(200));

            return true;
        }
        catch (RegexMatchTimeoutException)
        {
            return false;
        }
        catch (ArgumentException)
        {
            return false;
        }
    }

    // Examines the domain part of the email and normalizes it.
    private static string DomainMapper(Match match)
    {
        // Use IdnMapping class to convert Unicode domain names.
        var idn = new IdnMapping();

        // Pull out and process domain name (throws ArgumentException on invalid)
        var domainName = idn.GetAscii(match.Groups[2].Value);

        return match.Groups[1].Value + domainName;
    }
}