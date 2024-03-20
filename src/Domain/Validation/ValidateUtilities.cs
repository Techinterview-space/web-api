using System;
using System.Collections.Generic;
using System.Linq;
using Domain.Validation.Exceptions;

namespace Domain.Validation;

public static class ValidateUtilities
{
    public static T ThrowIfNull<T>(this T instance, string paramName, string customErrorMessage = null)
    {
        if (string.IsNullOrEmpty(paramName))
        {
            throw new InvalidOperationException("You should not pass null or empty string a paramName");
        }

        if (instance != null)
        {
            return instance;
        }

        var exception = customErrorMessage.NullOrEmpty()
            ? new ArgumentNullException(paramName: paramName)
            : new ArgumentNullException(paramName: paramName, message: customErrorMessage);

        throw exception;
    }

    public static T[] ThrowIfNullOrEmpty<T>(this T[] collection, string paramName)
    {
        paramName.ThrowIfNullOrEmpty(nameof(paramName));
        collection.ThrowIfNull(paramName);

        if (!collection.Any())
        {
            throw new InvalidOperationException($"You should not pass empty collection: {paramName}");
        }

        return collection;
    }

    public static IReadOnlyCollection<T> ThrowIfNullOrEmpty<T>(this IReadOnlyCollection<T> collection, string paramName)
    {
        paramName.ThrowIfNullOrEmpty(nameof(paramName));
        collection.ThrowIfNull(paramName);

        if (!collection.Any())
        {
            throw new InvalidOperationException($"You should not pass empty collection: {paramName}");
        }

        return collection;
    }

    public static void ThrowIfNullOrEmpty<T>(this ICollection<T> collection, string paramName)
    {
        (collection?.ToArray()).ThrowIfNullOrEmpty(paramName);
    }

    public static bool NullOrEmpty(this string @string)
    {
        return string.IsNullOrEmpty(@string?.Trim());
    }

    public static string ThrowIfNullOrEmpty(this string @string, string paramName)
    {
        if (paramName.NullOrEmpty())
        {
            throw new InvalidOperationException("You should not pass null or empty string a paramName");
        }

        if (@string.NullOrEmpty())
        {
            throw new BadRequestException("You should not pass null or empty string for " + paramName);
        }

        return @string;
    }

    public static bool IsDefaultValue(this double @double)
    {
        return Math.Abs(@double - default(double)) < 0.01;
    }
}