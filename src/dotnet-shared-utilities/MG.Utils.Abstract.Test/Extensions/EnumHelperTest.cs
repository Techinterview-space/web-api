using MG.Utils.Abstract.Extensions;
using Xunit;

namespace MG.Utils.Abstract.Test.Extensions
{
    public class EnumHelperTest
    {
        [Theory]
        [InlineData("Undefined", AwesomeEnum.Undefined)]
        [InlineData("undefined", AwesomeEnum.Undefined)]
        [InlineData("First", AwesomeEnum.First)]
        [InlineData("first", AwesomeEnum.First)]
        [InlineData("Second", AwesomeEnum.Second)]
        [InlineData("second", AwesomeEnum.Second)]
        [InlineData(" Undefined ", AwesomeEnum.Undefined)]
        [InlineData(" undefined ", AwesomeEnum.Undefined)]
        [InlineData(" First ", AwesomeEnum.First)]
        [InlineData(" first ", AwesomeEnum.First)]
        [InlineData(" Second ", AwesomeEnum.Second)]
        [InlineData(" second ", AwesomeEnum.Second)]
        public void ToEnum_ExistingValue_Ok(string @string, AwesomeEnum parsedValue)
        {
            Assert.Equal(parsedValue, @string.ToEnum<AwesomeEnum>());
        }

        [Fact]
        public void ToEnum_ValueDoesNotExist_ReturnsDefaultValue()
        {
            Assert.Equal(AwesomeEnum.Undefined, "Third".ToEnum<AwesomeEnum>());
        }

        [Fact]
        public void ToEnum_EmptyString_ReturnsDefaultValue()
        {
            Assert.Equal(AwesomeEnum.Undefined, string.Empty.ToEnum<AwesomeEnum>());
        }

        [Fact]
        public void ToEnum_Null_ReturnsDefaultValue()
        {
            Assert.Equal(AwesomeEnum.Undefined, ((string)null).ToEnum<AwesomeEnum>());
        }

        public enum AwesomeEnum
        {
            Undefined,
            First,
            Second
        }
    }
}