using System;
using System.Threading;
using System.Threading.Tasks;
using Domain.Enums;
using Domain.Services.Html;
using Domain.Services.Interviews;
using MG.Utils.Export.Pdf;
using Moq;
using TestUtils.Db;
using TestUtils.Fakes;
using Xunit;

namespace DomainTests.Services.Interviews;

public class InterviewPdfTest
{
    [Fact]
    public async Task RenderAsync_OkAsync()
    {
        await using var context = new SqliteContext();
        var user = await new FakeUser(Role.Interviewer, "Maxim", "Gorbatyuk", "m.gorbatyuk@gmail.com").PleaseAsync(context);
        var interview = await new InterviewFake(user).AsPlease().PleaseAsync(context);

        Assert.NotEqual(default(Guid), interview.Id);

        var expectedHtml = @$"<h1 id=""interview-with-jorn-smith"">Interview with Jorn Smith</h1>
<ul>
<li><p>Grade: <strong>Middle</strong></p>
</li>
<li><p>Interviewer: Maxim Gorbatyuk, m.gorbatyuk@gmail.com</p>
</li>
<li><p>When: {interview.CreatedAt.ToString(InterviewMarkdown.DateFormat)}</p>
</li>
</ul>
<h2 id=""overall-opinion"">Overall Opinion</h2>
<p>Open mind person</p>
<h2 id=""subjects"">Subjects</h2>
<h4 id=""asp.net-core-middle"">ASP.NET Core - Middle</h4>
<p>Middlewares, Caching</p>
<h4 id=""sql-did-not-check"">SQL - Did not check</h4>
<hr />";

        var pdfRenderer = new Mock<IPdf>();
        pdfRenderer.Setup(x => x.RenderAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Callback((string html, string _, string _, CancellationToken _) =>
            {
                Assert.StartsWith(expectedHtml, html);
            })
            .ReturnsAsync(new FileData(Array.Empty<byte>(), "test.pdf", FileData.PDfContentType));

        var file = await new InterviewPdf(
            new GlobalFake(),
            pdfRenderer.Object,
            new TechInterviewHtmlGenerator())
            .RenderAsync(interview);
        Assert.Equal("test.pdf", file.Filename);
        Assert.Equal(FileData.PDfContentType, file.ContentType);

        pdfRenderer.Verify(x => x.RenderAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}