using System;
using System.Collections.Generic;
using System.Linq;
using MG.Utils.Abstract.Dates;
using Xunit;

namespace MG.Utils.Abstract.Test
{
    public class ValidateUtilitiesTest
    {
        [Fact]
        public void ThrowIfNull_InstanceIsNull_ExpectedException_Ok()
        {
            Assert.Throws<ArgumentNullException>(() => (null as Date).ThrowIfNull("paramName"));

            Assert.Throws<ArgumentNullException>(() =>
                (null as ValidateUtilitiesTest).ThrowIfNull("paramName"));

            Assert.Throws<ArgumentNullException>(() =>
                (null as ICollection<string>).ThrowIfNull("paramName"));
        }

        [Theory]
        [InlineData("", true)]
        [InlineData(null, true)]
        [InlineData("ololo", false)]
        public void NullOrEmpty_Cases(string source, bool expected)
        {
            Assert.Equal(expected, source.NullOrEmpty());
        }

        [Fact]
        public void ThrowIfNullOrEmpty_Array_Empty_Exception()
        {
            Assert.Throws<InvalidOperationException>(() =>
                (Array.Empty<string>() as string[]).ThrowIfNullOrEmpty("paramName"));
        }

        [Fact]
        public void ThrowIfNullOrEmpty_IReadonlyCollection_Empty_Exception()
        {
            Assert.Throws<InvalidOperationException>(() =>
                (Array.Empty<string>().ToList() as IReadOnlyCollection<string>).ThrowIfNullOrEmpty("paramName"));
        }

        [Fact]
        public void ThrowIfNullOrEmpty_ICollection_Empty_Exception()
        {
            Assert.Throws<InvalidOperationException>(() =>
                (Array.Empty<string>().ToList() as ICollection<string>).ThrowIfNullOrEmpty("paramName"));
        }

        [Fact]
        public void ThrowIfNullOrEmpty_Array_Null_Exception()
        {
            Assert.Throws<ArgumentNullException>(() =>
                (null as string[]).ThrowIfNullOrEmpty("paramName"));
        }

        [Fact]
        public void ThrowIfNullOrEmpty_IReadonlyCollection_Null_Exception()
        {
            Assert.Throws<ArgumentNullException>(() =>
                (null as IReadOnlyCollection<string>).ThrowIfNullOrEmpty("paramName"));
        }

        [Fact]
        public void ThrowIfNullOrEmpty_ICollection_Null_Exception()
        {
            Assert.Throws<ArgumentNullException>(() =>
                (null as ICollection<string>).ThrowIfNullOrEmpty("paramName"));
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public void ThrowIfNullOrEmpty_ForString_Cases_Exception(string source)
        {
            Assert.Throws<ArgumentNullException>(() =>
                source.ThrowIfNullOrEmpty("paramName"));
        }
    }
}