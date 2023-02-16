using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using MG.Utils.AspNetCore.I18N.Core;
using MG.Utils.I18N;
using Moq;
using Xunit;

namespace MG.Utils.AspNetCore.I18N.Tests
{
    public class JsonFileLocalizerTest
    {
        [Fact]
        public void GetAllStrings_ForCultureRu_Ok()
        {
            var target = new JsonFileLocalizer(Settings("ru").Object);
            var list = target.GetAllStrings(true).ToList();

            Assert.Equal(4, list.Count);
            Assert.Equal("Привет", list[0].Value);
            Assert.Equal("Мир", list[1].Value);
            Assert.Equal("Компьютер", list[2].Value);
            Assert.Equal("Поле {0} верните", list[3].Value);
        }

        [Fact]
        public void GetAllStrings_ForCultureEn_Ok()
        {
            var target = new JsonFileLocalizer(Settings("en").Object);
            var list = target.GetAllStrings(true).ToList();

            Assert.Equal(4, list.Count);
            Assert.Equal("Hello", list[0].Value);
            Assert.Equal("World", list[1].Value);
            Assert.Equal("Computer", list[2].Value);
            Assert.Equal("Put the field {0} back", list[3].Value);
        }

        [Theory]
        [InlineData("ru", "HelloKey", "Привет")]
        [InlineData("en", "HelloKey", "Hello")]
        [InlineData("ru", "WorldKey", "Мир")]
        [InlineData("en", "WorldKey", "World")]
        [InlineData("ru", "ComputerKey", "Компьютер")]
        [InlineData("en", "ComputerKey", "Computer")]
        public void ThisName_Cases(string culture, string key, string expected)
        {
            var target = new JsonFileLocalizer(Settings(culture).Object);
            Assert.Equal(expected, target[key]);
        }

        [Theory]
        [InlineData("en", "hello world")]
        [InlineData("en", "here is a string with argument {0}")]
        [InlineData("ru", "привет мир")]
        [InlineData("ru", "вот строка с аргументом {0}")]
        public void ThisName_NoTranslations_KeyIsReturned_Cases(string culture, string key)
        {
            var target = new JsonFileLocalizer(Settings(culture).Object);
            Assert.Equal(key, target[key]);
        }

        [Theory]
        [InlineData("en", "WithArguments", "Email", "Put the field Email back")]
        [InlineData("ru", "WithArguments", "Email", "Поле Email верните")]
        [InlineData("en", "WithArguments", "Телефон", "Put the field Телефон back")]
        [InlineData("ru", "WithArguments", "Телефон", "Поле Телефон верните")]
        public void ThisName_WithArgument_KeyIsReturned_Cases(string culture, string key, string arg, string expected)
        {
            var target = new JsonFileLocalizer(Settings(culture).Object);
            Assert.Equal(expected, target[key, arg]);
        }

        public Mock<ILocalizationJsonSettings> Settings(string culture)
        {
            var mock = new Mock<ILocalizationJsonSettings>();
            mock.Setup(x => x.Translates())
                .Returns(new List<Translate>
                {
                    new Translate("HelloKey", "Hello", "Привет"),
                    new Translate("WorldKey", "World", "Мир"),
                    new Translate("ComputerKey", "Computer", "Компьютер"),
                    new Translate("WithArguments", "Put the field {0} back", "Поле {0} верните"),
                });

            mock.Setup(x => x.CultureInfo)
                .Returns(new CultureInfo(culture));

            return mock;
        }
    }
}