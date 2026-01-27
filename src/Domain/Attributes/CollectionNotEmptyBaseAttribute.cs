using System;
using System.Collections;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace Domain.Attributes;

[AttributeUsage(AttributeTargets.Property)]
public class CollectionNotEmptyBaseAttribute : ValidationAttribute
{
    private const string DefaultError = "The collection should not be empty";

    protected override ValidationResult IsValid(object value, ValidationContext validationContext)
    {
        return value switch
        {
            null => new ValidationResult(FormatError(validationContext)),
            IEnumerable enumerable => enumerable.GetEnumerator().MoveNext()
                ? ValidationResult.Success
                : new ValidationResult(FormatError(validationContext)),
            _ => throw new InvalidOperationException("Do not use this attribute for non-collection properties")
        };
    }

    public sealed override string FormatErrorMessage(string name)
    {
        return base.FormatErrorMessage(name);
    }

    protected virtual string FormatError([NotNull] ValidationContext validationContext)
    {
        return ErrorMessage ?? DefaultError;
    }
}