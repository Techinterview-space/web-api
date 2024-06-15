using System;
using System.Linq;
using System.Threading.Tasks;
using Domain.Entities.Questions;
using Domain.Enums;
using Domain.Validation.Exceptions;
using TestUtils.Auth;
using TestUtils.Db;
using TestUtils.Fakes;
using Web.Api.Features.Surveys.ReplyOnSalariesSurvey;
using Web.Api.Features.Surveys.Services;
using Xunit;

namespace Web.Api.Tests.Features.Survey.ReplyOnSalariesSurvey;

public class ReplyOnSalariesSurveyHandlerTests
{
    [Theory]
    [InlineData(ExpectationReplyType.Expected)]
    [InlineData(ExpectationReplyType.MoreThanExpected)]
    public async Task Handle_NoReplies_SavesReply(
        ExpectationReplyType replyType)
    {
        await using var context = new InMemoryDatabaseContext();

        var currentUser = await new FakeUser(Role.Interviewer).PleaseAsync(context);
        var auth = new FakeAuth(currentUser);

        var handler = new ReplyOnSalariesSurveyHandler(context, auth);

        context.ChangeTracker.Clear();
        Assert.Equal(0, context.SalariesSurveyReplies.Count());

        var result = await handler.Handle(
            new ReplyOnSalariesSurveyCommand
            {
                UsefulnessReply = SurveyUsefulnessReplyType.Yes,
                ExpectationReply = replyType,
            },
            default);

        Assert.Equal(1, context.SalariesSurveyReplies.Count());

        Assert.NotNull(result);
        Assert.Equal(replyType, result.ExpectationReply);
        Assert.Equal(currentUser.Id, result.CreatedByUserId);
    }

    [Fact]
    public async Task Handle_HasRecentReplies_Exception()
    {
        await using var context = new InMemoryDatabaseContext();

        var currentUser = await new FakeUser(Role.Interviewer).PleaseAsync(context);
        var auth = new FakeAuth(currentUser);

        var reply = await new FakeSalariesSurveyReply(
            SurveyUsefulnessReplyType.Yes,
            ExpectationReplyType.Expected,
            currentUser,
            DateTime.UtcNow.AddDays(-1)).PleaseAsync(context);

        var handler = new ReplyOnSalariesSurveyHandler(context, auth);

        context.ChangeTracker.Clear();
        Assert.Equal(1, context.SalariesSurveyReplies.Count());

        await Assert.ThrowsAsync<BadRequestException>(() =>
            handler.Handle(
                new ReplyOnSalariesSurveyCommand
                {
                    ExpectationReply = ExpectationReplyType.Expected,
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

        var oldDate = DateTime.UtcNow
            .AddDays(-SalariesSurveyUserService.RecentRepliesDays)
            .AddDays(-1);

        var reply = await new FakeSalariesSurveyReply(
            SurveyUsefulnessReplyType.Yes,
            ExpectationReplyType.Expected,
            currentUser,
            oldDate).PleaseAsync(context);

        var handler = new ReplyOnSalariesSurveyHandler(context, auth);

        context.ChangeTracker.Clear();
        Assert.Equal(1, context.SalariesSurveyReplies.Count());

        var result = await handler.Handle(
            new ReplyOnSalariesSurveyCommand
            {
                UsefulnessReply = SurveyUsefulnessReplyType.Yes,
                ExpectationReply = ExpectationReplyType.Expected,
            },
            default);

        Assert.Equal(2, context.SalariesSurveyReplies.Count());

        Assert.NotNull(result);
        Assert.Equal(ExpectationReplyType.Expected, result.ExpectationReply);
        Assert.Equal(currentUser.Id, result.CreatedByUserId);
    }
}