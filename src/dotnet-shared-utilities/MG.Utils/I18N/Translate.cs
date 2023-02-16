using System.Globalization;
using MG.Utils.Abstract;
using MG.Utils.Helpers;

namespace MG.Utils.I18N
{
    public record Translate
    {
        public Translate()
        {
        }

        public Translate(string key, string en, string ru)
        {
            key.ThrowIfNullOrEmpty(nameof(key));
            en.ThrowIfNullOrEmpty(nameof(en));
            ru.ThrowIfNullOrEmpty(nameof(ru));

            Key = key;
            this.en = en;
            this.ru = ru;
        }

        public string Key { get; set; }

#pragma warning disable SA1300, IDE1006
        public string en { private get; set; }

        public string ru { private get; set; }
#pragma warning restore SA1300, IDE1006

        public string TranslationByName(CultureInfo cultureInfo)
        {
            if (!HasAnyValue)
            {
                return Key;
            }

            return cultureInfo.TwoLetterISOLanguageName switch
            {
                nameof(ru) => !ru.IsNullOrEmpty() ? ru : en,
                _ => !en.IsNullOrEmpty() ? en : Key
            };
        }

        public bool HasAnyValue => !en.IsNullOrEmpty() || !ru.IsNullOrEmpty();

        public bool IsValid() => !en.IsNullOrEmpty() && !ru.IsNullOrEmpty();

        public override string ToString()
        {
            return $"{nameof(Translate)}. Key:{Key}; En:{en}; Ru:{ru}";
        }
    }
}