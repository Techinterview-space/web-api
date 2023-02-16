using System;
using System.IO;
using Microsoft.AspNetCore.Http;
using Moq;

namespace TestUtils.Mocks;

public class FileAttachmentMock : Mock<IFormFile>
{
    private const int Kilo = 1024;

    public const string PdfContentType = "application/pdf";
    public const string PngContentType = "image/png";
    public const string GifContentType = "image/gif";
    public const string JpegContentType = "image/jpg";

    public FileAttachmentMock(
        string fileName,
        int fileSizeInMegabytes = 6)
    {
        Setup(x => x.ContentType).Returns(ContentType(fileName));
        Setup(x => x.FileName).Returns(fileName);
        Setup(x => x.Length).Returns(fileSizeInMegabytes * Kilo * Kilo);
        Setup(x => x.OpenReadStream()).Returns(new MemoryStream(Array.Empty<byte>()));
    }

    private static string ContentType(
        string fileName)
    {
        if (fileName.EndsWith("pdf"))
        {
            return PdfContentType;
        }

        if (fileName.EndsWith("png"))
        {
            return PngContentType;
        }

        if (fileName.EndsWith("gif"))
        {
            return GifContentType;
        }

        if (fileName.EndsWith("jpg") || fileName.EndsWith("jpeg"))
        {
            return JpegContentType;
        }

        throw new ArgumentException($"File name {fileName} is not supported");
    }
}