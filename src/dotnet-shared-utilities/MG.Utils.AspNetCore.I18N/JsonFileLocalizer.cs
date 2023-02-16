using System;
using System.Collections.Generic;
using System.Linq;
using MG.Utils.AspNetCore.I18N.Core;
using Microsoft.Extensions.Localization;

namespace MG.Utils.AspNetCore.I18N
{
    public class JsonFileLocalizer : IStringLocalizer
    {
        private readonly ILocalizationJsonSettings _settings;

        public JsonFileLocalizer(ILocalizationJsonSettings settings)
        {
            _settings = settings;
        }

        public IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures)
        {
            return _settings.Translates()
                .Select(r => new LocalizedString(
                    name: r.Key,
                    value: r.TranslationByName(_settings.CultureInfo)));
        }

        public LocalizedString this[string name]
        {
            get
            {
                var value = Translate(name);
                return new LocalizedString(name, value ?? name, resourceNotFound: value == null);
            }
        }

        public LocalizedString this[string name, params object[] arguments]
        {
            get
            {
                (string value, string format) = FormatOrFail(name, arguments);

                return new LocalizedString(name, value, resourceNotFound: format == null);
            }
        }

        private (string value, string format) FormatOrFail(string name, params object[] arguments)
        {
            string format = Translate(name) ?? name;

            try
            {
                return (string.Format(format, arguments), format);
            }
            catch (FormatException e)
            {
                throw new InvalidOperationException(
                    $"Could not use a string '{format}' as format template", e);
            }
        }

        private string Translate(string name)
        {
            return _settings.Translates()
                .FirstOrDefault(r => r.Key == name)?
                .TranslationByName(_settings.CultureInfo);
        }
    }
}