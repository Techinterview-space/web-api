using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Http;

namespace Web.Api.Setup.Attributes;

// copied from https://stackoverflow.com/questions/56588900/how-to-validate-uploaded-file-in-asp-net-core
public class AllowedExtensionsAttribute : ValidationAttribute
{
    private readonly IReadOnlyCollection<string> _extensions;

    public AllowedExtensionsAttribute(params string[] extensions)
    {
        _extensions = extensions;
    }

    protected override ValidationResult IsValid(object value, ValidationContext validationContext)
    {
        if (value is null)
        {
            return ValidationResult.Success;
        }

        if (value is IFormFile file)
        {
            var extension = Path.GetExtension(file.FileName);

            if (extension == null)
            {
                return new ValidationResult(FormatError(validationContext));
            }

            if (extension.StartsWith("."))
            {
                extension = extension[1..extension.Length];
            }

            if (!_extensions.Contains(extension.ToLower()))
            {
                return new ValidationResult(FormatError(validationContext));
            }

            return ValidationResult.Success;
        }

        throw new InvalidOperationException("You should use this attribute only for File properties");
    }

    public sealed override string FormatErrorMessage(string name)
    {
        return base.FormatErrorMessage(name);
    }

    protected virtual string FormatError([NotNull] ValidationContext validationContext)
    {
        return ErrorMessage ?? "The passed file has disallowed extension";
    }
}