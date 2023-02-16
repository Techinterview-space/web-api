using System;
using MG.Utils.Abstract.NonNullableObjects;
using Xunit;

namespace MG.Utils.Abstract.Test.NonNullableObjects
{
    public class NonNullableTest
    {
        [Theory]
        [InlineData("")]
        [InlineData("ololo")]
        [InlineData("1")]
        [InlineData("   ")]
        public void Value_String_HasSource_Ok(string source)
        {
            Assert.Equal(source, new NonNullable<string>(source).Value());
        }

        [Fact]
        public void Value_NullString_HasNoSource_Exception()
        {
            Assert.Throws<InvalidOperationException>(() => new NonNullable<string>(null).Value());

            Assert.Throws<InvalidOperationException>(() => new NonNullable<object>(null).Value());
        }
    }
}