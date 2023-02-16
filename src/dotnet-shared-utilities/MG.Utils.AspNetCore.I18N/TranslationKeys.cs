using System;
using System.Collections.Generic;
using System.Linq;
using MG.Utils.Helpers;
using MG.Utils.I18N;

namespace MG.Utils.AspNetCore.I18N
{
    public class TranslationKeys
    {
        public IReadOnlyCollection<string> KeysFromConstantsWithoutTranslations =>
            _keysFromConstantsWithoutTranslations.AsReadOnly();

        public IReadOnlyCollection<string> KeysFromTranslationsWithoutConstantKey =>
            _keysFromTranslationsWithoutConstantKey.AsReadOnly();

        private readonly IReadOnlyCollection<Translate> _translates;
        private readonly Constants _constants;
        private readonly Type _typeOfConstants;

        private readonly List<string> _keysFromConstantsWithoutTranslations = new ();
        private readonly List<string> _keysFromTranslationsWithoutConstantKey = new ();

        public TranslationKeys(IReadOnlyCollection<Translate> translates, Type typeOfConstants)
        {
            _translates = translates;
            _constants = new Constants(typeOfConstants);
            _typeOfConstants = typeOfConstants;
        }

        public void MatchOrFail()
        {
            string[] translationKeys = _translates.Select(x => x.Key).ToArray();
            IReadOnlyCollection<string> constantKeys = _constants.Names();

            _keysFromTranslationsWithoutConstantKey.AddRange(translationKeys.Except(constantKeys));
            _keysFromConstantsWithoutTranslations.AddRange(constantKeys.Except(translationKeys));

            if (!_keysFromConstantsWithoutTranslations.Any() && !_keysFromTranslationsWithoutConstantKey.Any())
            {
                return;
            }

            string message = string.Empty;

            if (_keysFromConstantsWithoutTranslations.Any())
            {
                message += $"There are {_keysFromConstantsWithoutTranslations.Count} keys from {_typeOfConstants.Name} class without translation from the file{Environment.NewLine}";

                foreach (string key in _keysFromConstantsWithoutTranslations)
                {
                    message += key + Environment.NewLine;
                }

                message += Environment.NewLine;
            }

            if (_keysFromTranslationsWithoutConstantKey.Any())
            {
                message += $"There are {_keysFromTranslationsWithoutConstantKey.Count} keys from JSON file without Key from {_typeOfConstants.Name} class{Environment.NewLine}";

                foreach (string key in _keysFromTranslationsWithoutConstantKey)
                {
                    message += key + Environment.NewLine;
                }
            }

            throw new InvalidOperationException(message);
        }
    }
}