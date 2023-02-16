using System;
using System.Collections.Generic;
using System.Linq;
using MG.Utils.I18N;
using Xunit;

namespace MG.Utils.AspNetCore.I18N.Tests
{
    public class TranslationKeysTest
    {
        [Fact]
        public void MatchOrFail_SameCollection_Ok()
        {
            var target = new TranslationKeys(
                new List<Translate>
                {
                    new Translate("Hello", "Hello", "Привет"),
                    new Translate("World", "World", "Мир")
                },
                typeof(AwesomeConstants));

            // No exception is ok
            target.MatchOrFail();

            Assert.Empty(target.KeysFromConstantsWithoutTranslations);
            Assert.Empty(target.KeysFromTranslationsWithoutConstantKey);
        }

        [Fact]
        public void MatchOrFail_FileHasMoreKeys_Exception()
        {
            var target = new TranslationKeys(
                new List<Translate>
                {
                    new Translate("Hello", "Hello", "Привет"),
                    new Translate("World", "World", "Мир"),
                    new Translate("Me", "Me", "я")
                },
                typeof(AwesomeConstants));

            Assert.Throws<InvalidOperationException>(() => target.MatchOrFail());

            Assert.Empty(target.KeysFromConstantsWithoutTranslations);
            Assert.Single(target.KeysFromTranslationsWithoutConstantKey);

            Assert.Equal("Me", target.KeysFromTranslationsWithoutConstantKey.First());
        }

        [Fact]
        public void MatchOrFail_ConstantClassHasMoreKeys_Exception()
        {
            var target = new TranslationKeys(
                new List<Translate>
                {
                    new Translate("Hello", "Hello", "Привет"),
                    new Translate("World", "World", "Мир"),
                },
                typeof(AwesomeConstantsExtended));

            Assert.Throws<InvalidOperationException>(() => target.MatchOrFail());

            Assert.Empty(target.KeysFromTranslationsWithoutConstantKey);
            Assert.Single(target.KeysFromConstantsWithoutTranslations);

            Assert.Equal("Me", target.KeysFromConstantsWithoutTranslations.First());
        }

        [Fact]
        public void MatchOrFail_NoIntersections_Exception()
        {
            var target = new TranslationKeys(
                new List<Translate>
                {
                    new Translate("Krosh", "Krosh", "Крош"),
                    new Translate("Pin", "Pin", "Пин"),
                },
                typeof(AwesomeConstants));

            Assert.Throws<InvalidOperationException>(() => target.MatchOrFail());

            Assert.Equal(2, target.KeysFromTranslationsWithoutConstantKey.Count);
            Assert.Equal(2, target.KeysFromConstantsWithoutTranslations.Count);
        }

        public class AwesomeConstants
        {
            public const string Hello = nameof(Hello);

            public const string World = nameof(World);
        }

        public class AwesomeConstantsExtended : AwesomeConstants
        {
            public const string Me = nameof(Me);
        }
    }
}