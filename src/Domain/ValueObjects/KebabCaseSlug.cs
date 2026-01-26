using System;
using System.Text.RegularExpressions;

namespace Domain.ValueObjects;

public record KebabCaseSlug
{
    private readonly string _source;
    private readonly string _result;

    /// <summary>
    /// Initializes a new instance of the <see cref="KebabCaseSlug"/> class.
    /// </summary>
    /// <param name="source">Source.</param>
    public KebabCaseSlug(
        string source)
    {
        source = source?
            .Trim()
            .Trim('-')
            .Trim('_');

        if (string.IsNullOrEmpty(source))
        {
            throw new InvalidOperationException("Slug source cannot be null or empty");
        }

        _source = source;

        // Replace spaces and underscores with hyphens
        _result = Regex.Replace(_source, @"[()""._\- ]+", "-");
        _result = _result.Trim('-');

        // Insert hyphen before uppercase letters (except the first character)
        _result = Regex.Replace(_result, @"([a-z0-9])([A-Z])", "$1-$2").ToLowerInvariant();
    }

    public override string ToString()
    {
        return _result;
    }

    /// <summary>
    /// This method is used to convert the KebabCaseSlug to a string.
    /// </summary>
    /// <param name="slug">Slug.</param>
    public static explicit operator string(
        KebabCaseSlug slug)
    {
        return slug.ToString();
    }
}