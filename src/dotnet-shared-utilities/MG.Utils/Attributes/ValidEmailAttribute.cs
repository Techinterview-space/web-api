using System;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using MG.Utils.Validation;

namespace MG.Utils.Attributes
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public class ValidEmailAttribute : ValidationAttribute
    {
        private static readonly EmailValidRegex _emailRegex = new ();

        public sealed override string FormatErrorMessage(string name)
        {
            return base.FormatErrorMessage(name);
        }

        protected virtual string FormatError([NotNull] ValidationContext validationContext)
        {
            return ErrorMessage ?? "Invalid email";
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            // ValidEmailAttribute doesn't necessarily mean required
            if (value == null)
            {
                return ValidationResult.Success;
            }

            if (value is not string valueString)
            {
                return new ValidationResult(FormatError(validationContext));
            }

            return _emailRegex.IsValid(valueString)
                ? ValidationResult.Success
                : new ValidationResult(FormatError(validationContext));
        }
    }
}