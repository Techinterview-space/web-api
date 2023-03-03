using System.Text;
using System.Threading.Tasks;
using DinkToPdf.Contracts;
using Domain.Files;
using Microsoft.Extensions.Logging;
using Moq;
using TechInterviewer.Services.PDF;
using Xunit;

namespace Web.Api.Tests.Services.PDF
{
    public class PdfRendererTests
    {
        [Fact]
        public async Task AsByteArray_ReturnsByteArrayAsync()
        {
            const string testingText = "<p>Html Content</p>";

            var logger = new Mock<ILogger<PdfRenderer>>();
            var converterMoq = new Mock<IDisposableConverter>();
            converterMoq.Setup(x => x.Convert(It.IsAny<IDocument>()))
                .Returns(Encoding.ASCII.GetBytes(testingText));

            var htmlToPdf = new PdfRenderer(converterMoq.Object, null, null, logger.Object);
            var result = await htmlToPdf.RenderAsync(testingText, "test.pdf", FileData.PDfContentType);

            Assert.Equal(testingText, Encoding.ASCII.GetString(result.Data));
            Assert.Equal("test.pdf", result.Filename);
            Assert.Equal(FileData.PDfContentType, result.ContentType);
        }
    }
}
