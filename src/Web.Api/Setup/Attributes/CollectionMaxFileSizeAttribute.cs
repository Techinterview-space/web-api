using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Microsoft.AspNetCore.Http;

namespace Web.Api.Setup.Attributes;

public class CollectionMaxFileSizeAttribute : MaxFileSizeAttribute
{
    public CollectionMaxFileSizeAttribute(int megabytes)
        : base(megabytes)
    {
    }

    protected override ValidationResult IsValid(
        object value,
        ValidationContext validationContext)
    {
        // cast failed, not a list
        if (value is not IEnumerable<IFormFile> files)
        {
            return base.IsValid(value, validationContext);
        }

        return files.Any(x => x.Length > MaxFileSizeInBytes)
            ? new ValidationResult(FormatError(validationContext))
            : ValidationResult.Success;
    }
}