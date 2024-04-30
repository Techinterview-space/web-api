using System;
using System.Linq;
using System.Threading.Tasks;
using Domain.Entities.Questions;
using Domain.Enums;
using Domain.Validation.Exceptions;
using TechInterviewer.Features.Surveys.ReplyOnSalariesSurvey;
using TestUtils.Auth;
using TestUtils.Db;
using TestUtils.Fakes;
using Xunit;

namespace Web.Api.Tests.Features.Survey.ReplyOnSalariesSurvey;

public class ReplyOnSalariesSurveyHandlerTests
{
    [Theory]
    [InlineData(SalariesSurveyReplyType.Expected)]
    [InlineData(SalariesSurveyReplyType.DidNotExpected)]
    public async Task Handle_NoReplies_SavesReply(
        SalariesSurveyReplyType replyType)
    {
        await using var context = new InMemoryDatabaseContext();

        var currentUser = await new FakeUser(Role.Interviewer).PleaseAsync(context);
        var auth = new FakeAuth(currentUser);

        Assert.Equal(1, context.SalariesSurveyQuestions.Count());
        var question = context.SalariesSurveyQuestions.First();

        var handler = new ReplyOnSalariesSurveyHandler(context, auth);

        context.ChangeTracker.Clear();
        Assert.Equal(1, context.SalariesSurveyQuestions.Count());
        Assert.Equal(0, context.SalariesSurveyReplies.Count());

        var result = await handler.Handle(
            new ReplyOnSalariesSurveyCommand
            {
                SalariesSurveyQuestionId = question.Id,
                ReplyType = replyType,
            },
            default);

        Assert.Equal(1, context.SalariesSurveyReplies.Count());

        Assert.NotNull(result);
        Assert.Equal(question.Id, result.SalariesSurveyQuestionId);
        Assert.Equal(replyType, result.ReplyType);
        Assert.Equal(currentUser.Id, result.CreatedByUserId);
    }

    [Fact]
    public async Task Handle_HasRecentReplies_Exception()
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
            DateTime.UtcNow.AddDays(-1)).PleaseAsync(context);

        var handler = new ReplyOnSalariesSurveyHandler(context, auth);

        context.ChangeTracker.Clear();
        Assert.Equal(1, context.SalariesSurveyQuestions.Count());
        Assert.Equal(1, context.SalariesSurveyReplies.Count());

        await Assert.ThrowsAsync<BadRequestException>(() =>
            handler.Handle(
                new ReplyOnSalariesSurveyCommand
                {
                    SalariesSurveyQuestionId = question.Id,
                    ReplyType = SalariesSurveyReplyType.Expected,
                },
                default));

        Assert.Equal(1, context.SalariesSurveyReplies.Count());
    }

    [Fact]
    public async Task Handle_HasOldReplies_Exception()
    {
        await using var context = new InMemoryDatabaseContext();

        var currentUser = await new FakeUser(Role.Interviewer).PleaseAsync(context);
        var auth = new FakeAuth(currentUser);

        Assert.Equal(1, context.SalariesSurveyQuestions.Count());
        var question = context.SalariesSurveyQuestions.First();

        var oldDate = DateTime.UtcNow
            .AddDays(-ReplyOnSalariesSurveyHandler.RecentRepliesDays)
            .AddDays(-1);

        var reply = await new FakeSalariesSurveyReply(
            SalariesSurveyReplyType.Expected,
            question,
            currentUser,
            oldDate).PleaseAsync(context);

        var handler = new ReplyOnSalariesSurveyHandler(context, auth);

        context.ChangeTracker.Clear();
        Assert.Equal(1, context.SalariesSurveyQuestions.Count());
        Assert.Equal(1, context.SalariesSurveyReplies.Count());

        var result = await handler.Handle(
            new ReplyOnSalariesSurveyCommand
            {
                SalariesSurveyQuestionId = question.Id,
                ReplyType = SalariesSurveyReplyType.Expected,
            },
            default);

        Assert.Equal(2, context.SalariesSurveyReplies.Count());

        Assert.NotNull(result);
        Assert.Equal(question.Id, result.SalariesSurveyQuestionId);
        Assert.Equal(SalariesSurveyReplyType.Expected, result.ReplyType);
        Assert.Equal(currentUser.Id, result.CreatedByUserId);
    }
}