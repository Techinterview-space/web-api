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
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    [InlineData(5)]
    public async Task Handle_NoReplies_SavesReply(
        int ratingValue)
    {
        await using var context = new InMemoryDatabaseContext();

        var currentUser = await new UserFake(Role.Interviewer).PleaseAsync(context);
        var auth = new FakeAuth(currentUser);

        var handler = new ReplyOnSalariesSurveyHandler(context, auth);

        context.ChangeTracker.Clear();
        Assert.Equal(0, context.SalariesSurveyReplies.Count());

        var result = await handler.Handle(
            new ReplyOnSalariesSurveyCommand
            {
                UsefulnessRating = ratingValue,
            },
            default);

        Assert.Equal(1, context.SalariesSurveyReplies.Count());

        Assert.NotNull(result);
        Assert.Equal(ratingValue, result.UsefulnessRating);
        Assert.Equal(currentUser.Id, result.CreatedByUserId);
    }

    [Fact]
    public async Task Handle_HasRecentReplies_Exception()
    {
        await using var context = new InMemoryDatabaseContext();

        var currentUser = await new UserFake(Role.Interviewer).PleaseAsync(context);
        var auth = new FakeAuth(currentUser);

        var reply = await new SalariesSurveyReplyFake(
            5,
            currentUser,
            DateTime.UtcNow.AddDays(-1)).PleaseAsync(context);

        var handler = new ReplyOnSalariesSurveyHandler(context, auth);

        context.ChangeTracker.Clear();
        Assert.Equal(1, context.SalariesSurveyReplies.Count());

        await Assert.ThrowsAsync<BadRequestException>(() =>
            handler.Handle(
                new ReplyOnSalariesSurveyCommand
                {
                    UsefulnessRating = 4,
                },
                default));

        Assert.Equal(1, context.SalariesSurveyReplies.Count());
    }

    [Fact]
    public async Task Handle_HasOldReplies_Ok()
    {
        await using var context = new InMemoryDatabaseContext();

        var currentUser = await new UserFake(Role.Interviewer).PleaseAsync(context);
        var auth = new FakeAuth(currentUser);

        var oldDate = DateTime.UtcNow
            .AddDays(-SalariesSurveyUserService.RecentRepliesDays)
            .AddDays(-1);

        var reply = await new SalariesSurveyReplyFake(
            5,
            currentUser,
            oldDate).PleaseAsync(context);

        var handler = new ReplyOnSalariesSurveyHandler(context, auth);

        context.ChangeTracker.Clear();
        Assert.Equal(1, context.SalariesSurveyReplies.Count());

        var result = await handler.Handle(
            new ReplyOnSalariesSurveyCommand
            {
                UsefulnessRating = 5,
            },
            default);

        Assert.Equal(2, context.SalariesSurveyReplies.Count());

        Assert.NotNull(result);
        Assert.Equal(5, result.UsefulnessRating);
        Assert.Equal(currentUser.Id, result.CreatedByUserId);
    }
}