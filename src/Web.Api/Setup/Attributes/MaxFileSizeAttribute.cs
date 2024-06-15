using System;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Http;

namespace Web.Api.Setup.Attributes;

// copied from https://stackoverflow.com/questions/56588900/how-to-validate-uploaded-file-in-asp-net-core
public class MaxFileSizeAttribute : ValidationAttribute
{
    private const int Kilo = 1024;

    // In bytes
    protected int MaxFileSizeInBytes { get; }

    protected int MaxSizeInMegabytes { get; }

    public MaxFileSizeAttribute(int megabytes)
    {
        MaxSizeInMegabytes = megabytes;
        MaxFileSizeInBytes = megabytes * Kilo * Kilo;
    }

    protected override ValidationResult IsValid(object value, ValidationContext validationContext)
    {
        if (value == null)
        {
            return ValidationResult.Success;
        }

        return value switch
        {
            IFormFile file => file.Length > MaxFileSizeInBytes
                ? new ValidationResult(FormatError(validationContext))
                : ValidationResult.Success,
            _ => throw new InvalidOperationException("Use this attribute only for file properties")
        };
    }

    public sealed override string FormatErrorMessage(string name)
    {
        return base.FormatErrorMessage(name);
    }

    protected virtual string FormatError([NotNull] ValidationContext validationContext)
    {
        return ErrorMessage ?? "The file size is bigger than allowed one";
    }
}