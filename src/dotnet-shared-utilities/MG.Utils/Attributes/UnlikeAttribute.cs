using System;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace MG.Utils.Attributes
{
    // copied from https://stackoverflow.com/a/35208420
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class UnlikeAttribute : ValidationAttribute
    {
        public string OtherProperty { get; }

        public UnlikeAttribute(string otherProperty)
        {
            if (string.IsNullOrEmpty(otherProperty))
            {
                throw new ArgumentNullException(nameof(otherProperty));
            }

            OtherProperty = otherProperty;
        }

        public sealed override string FormatErrorMessage(string name)
        {
            return string.Format(ErrorMessageString, name, OtherProperty);
        }

        protected virtual string FormatError([NotNull] ValidationContext validationContext)
        {
            return ErrorMessage ?? $"The property value should not be equal to {OtherProperty}'s value";
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value == null)
            {
                return ValidationResult.Success;
            }

            var otherProperty = validationContext.ObjectInstance.GetType()
                .GetProperty(OtherProperty);

            var otherPropertyValue = otherProperty!.GetValue(validationContext.ObjectInstance, null);

            return value.Equals(otherPropertyValue)
                ? new ValidationResult(FormatError(validationContext))
                : ValidationResult.Success;
        }
    }
}