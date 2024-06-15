using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Http;

namespace Web.Api.Setup.Attributes;

public class CollectionStandardAllowedFileExtensionsAttribute : StandardAllowedFileExtensionsAttribute
{
    protected override ValidationResult IsValid(
        object value,
        ValidationContext validationContext)
    {
        // cast failed, not a list
        if (value is not IEnumerable<IFormFile> files)
        {
            return base.IsValid(value, validationContext);
        }

        return files.Any(x =>
        {
            var extension = Path.GetExtension(x.FileName);
            return extension == null || !_extensions.Contains(extension.ToLower());
        })
            ? new ValidationResult(this.FormatError(validationContext))
            : ValidationResult.Success;
    }
}