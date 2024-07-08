using System;
using System.Threading.Tasks;
using Domain.Enums;
using Infrastructure.Services.PDF.Interviews;
using TestUtils.Db;
using TestUtils.Fakes;
using Xunit;

namespace InfrastructureTests.Services.Interviews;

public class InterviewMarkdownTest
{
    [Fact]
    public async Task ToString_OkAsync()
    {
        await using var context = new SqliteContext();
        var user = await new FakeUser(Role.Interviewer, "Maxim", "Gorbatyuk", "m.gorbatyuk@gmail.com").PleaseAsync(context);
        var interview = await new InterviewFake(user).AsPlease().PleaseAsync(context);

        Assert.NotEqual(default(Guid), interview.Id);

        var markdown = new InterviewMarkdownBody(interview).ToString();

        var expected = @$"# Interview with Jorn Smith

- Grade: **Middle**

- Interviewer: Maxim Gorbatyuk, m.gorbatyuk@gmail.com

- When: {interview.CreatedAt.ToString(InterviewMarkdownBody.DateFormat)}

## Overall Opinion

Open mind person

## Subjects

#### ASP.NET Core - Middle

Middlewares, Caching

#### SQL - Did not check

";
        Assert.Equal(expected, markdown);
    }
}