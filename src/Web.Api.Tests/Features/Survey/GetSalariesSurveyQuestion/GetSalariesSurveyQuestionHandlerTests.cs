using System;
using System.Linq;
using System.Threading.Tasks;
using Domain.Entities.Questions;
using Domain.Enums;
using TechInterviewer.Features.Surveys.GetSalariesSurveyQuestion;
using TestUtils.Auth;
using TestUtils.Db;
using TestUtils.Fakes;
using Xunit;

namespace Web.Api.Tests.Features.Survey.GetSalariesSurveyQuestion;

public class GetSalariesSurveyQuestionHandlerTests
{
    [Fact]
    public async Task Handle_NoReplies_ReturnsTrue()
    {
        await using var context = new InMemoryDatabaseContext();

        var currentUser = await new FakeUser(Role.Interviewer).PleaseAsync(context);
        var auth = new FakeAuth(currentUser);

        var handler = new GetSalariesSurveyQuestionHandler(context, auth);

        context.ChangeTracker.Clear();
        Assert.Equal(1, context.SalariesSurveyQuestions.Count());

        var result = await handler.Handle(
            new GetSalariesSurveyQuestionQuery(), default);

        Assert.Equal(1, context.SalariesSurveyQuestions.Count());
        var question = context.SalariesSurveyQuestions.First();

        Assert.NotNull(result);
        Assert.Equal(question.Id, result.Question.Id);
        Assert.Equal(question.Title, result.Question.Title);
        Assert.Equal(question.Description, result.Question.Description);
        Assert.True(result.RequiresReply);
    }

    [Theory]
    [InlineData(1, false)]
    [InlineData(100, true)]
    public async Task Handle_HasReplies_ReturnsTrue(
        int daysToSubstract,
        bool expectedValue)
    {
        await using var context = new InMemoryDatabaseContext();

        var currentUser = await new FakeUser(Role.Interviewer).PleaseAsync(context);
        var auth = new FakeAuth(currentUser);

        Assert.Equal(1, context.SalariesSurveyQuestions.Count());
        var question = context.SalariesSurveyQuestions.First();

        var reply = await new FakeSalariesSurveyReply(
            SalariesSurveyReplyType.Expected,
            question,
            currentUser,
            DateTime.UtcNow.AddDays(-daysToSubstract)).PleaseAsync(context);

        var handler = new GetSalariesSurveyQuestionHandler(context, auth);

        context.ChangeTracker.Clear();
        var result = await handler.Handle(
            new GetSalariesSurveyQuestionQuery(), default);

        Assert.NotNull(result);
        Assert.Equal(question.Id, result.Question.Id);
        Assert.Equal(question.Title, result.Question.Title);
        Assert.Equal(question.Description, result.Question.Description);
        Assert.Equal(expectedValue, result.RequiresReply);
    }
}