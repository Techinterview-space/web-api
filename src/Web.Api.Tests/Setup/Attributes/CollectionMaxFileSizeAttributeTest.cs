using System.Collections.Generic;
using Domain.Validation;
using Domain.Validation.Exceptions;
using Microsoft.AspNetCore.Http;
using Moq;
using Web.Api.Setup.Attributes;
using Xunit;

namespace Web.Api.Tests.Setup.Attributes;

public class CollectionMaxFileSizeAttributeTest
{
    [Theory]
    [InlineData("hello.pdf", 1 * 1024 * 1024)]
    [InlineData("hello.docx", 5 * 1024 * 1024)]
    [InlineData("hello.zip", 7 * 1024 * 1024)]
    [InlineData("hello.xlsx", 8 * 1024 * 1024)]
    public void ValidCases_Ok(string filename, int length)
    {
        var fileMock = new Mock<IFormFile>();
        fileMock.Setup(x => x.FileName).Returns(filename);
        fileMock.Setup(x => x.Length).Returns(length);

        var target = new Request
        {
            Files = new List<IFormFile>
            {
                fileMock.Object
            }
        };

        // No exceptions is ok
        target.ThrowIfInvalid();
    }

    [Theory]
    [InlineData("hello.asdasda", 15 * 1024 * 1024)]
    [InlineData("hello.asdasda", 2 * 1024 * 1024)]
    [InlineData("hello.png", 15 * 1024 * 1024)]
    public void InvalidCases_Exception(string filename, int length)
    {
        var fileMock = new Mock<IFormFile>();
        fileMock.Setup(x => x.FileName).Returns(filename);
        fileMock.Setup(x => x.Length).Returns(length);

        var target = new Request
        {
            Files = new List<IFormFile>
            {
                fileMock.Object
            }
        };

        Assert.Throws<EntityInvalidException>(() => target.ThrowIfInvalid());
    }

    public record Request
    {
        [CollectionMaxFileSize(megabytes: 8)]
        [CollectionStandardAllowedFileExtensions]
        public IReadOnlyCollection<IFormFile> Files { get; init; }
    }
}