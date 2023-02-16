using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;

namespace MG.Utils.AspNetCore.Validation
{
#nullable enable
    public static class ValidationContextExtensions
    {
        public static string? ErrorMessage(this ValidationContext? validationContext, string errorMessage)
        {
            if (errorMessage == null)
            {
                throw new ArgumentNullException(nameof(errorMessage));
            }

            IStringLocalizer? localizer = validationContext?.GetService<IStringLocalizer>();

            if (localizer != null)
            {
                return localizer[errorMessage];
            }

            return errorMessage;
        }

        public static string? ErrorMessageWithDisplayName(this ValidationContext? validationContext, string errorMessage)
        {
            if (errorMessage == null)
            {
                throw new ArgumentNullException(nameof(errorMessage));
            }

            if (validationContext == null)
            {
                return errorMessage;
            }

            IStringLocalizer? localizer = validationContext.GetService<IStringLocalizer>();
            string displayName = validationContext.DisplayName;

            if (localizer != null)
            {
                return localizer[errorMessage, displayName];
            }

            return string.Format(errorMessage, displayName);
        }

        public static string? ErrorMessage(
            this ValidationContext? validationContext, string errorMessage, params object[] args)
        {
            if (errorMessage == null)
            {
                throw new ArgumentNullException(nameof(errorMessage));
            }

            IStringLocalizer? localizer = validationContext?.GetService<IStringLocalizer>();

            if (localizer != null)
            {
                return localizer[errorMessage, args];
            }

            return string.Format(errorMessage, args);
        }
    }
}