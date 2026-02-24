using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Domain.Entities.Surveys;
using Domain.Enums;
using Domain.Validation.Exceptions;
using TestUtils.Auth;
using TestUtils.Db;
using TestUtils.Fakes;
using Web.Api.Features.PublicSurveys.GetPublicSurveyResults;
using Web.Api.Features.PublicSurveys.SubmitPublicSurveyResponse;
using Xunit;

namespace Web.Api.Tests.Features.PublicSurveys.GetPublicSurveyResults;

public class GetPublicSurveyResultsHandlerTests
{
    [Fact]
    public async Task GetResults_AfterResponding_Ok()
    {
        await using var context = new InMemoryDatabaseContext();
        var author = await new UserFake(Role.Interviewer).PleaseAsync(context);
        var respondent = await new UserFake(Role.Interviewer).PleaseAsync(context);
        var (survey, question) = CreatePublishedSurvey(author, context);

        var optionA = question.Options.First(o => o.Text == "A");

        await SubmitResponse(context, respondent, "test-slug", new List<Guid> { optionA.Id });
        context.ChangeTracker.Clear();

        var result = await new GetPublicSurveyResultsHandler(context, new FakeAuth(respondent))
            .Handle("test-slug", default);

        Assert.Equal(1, result.TotalResponses);
        Assert.Equal(2, result.Options.Count);

        var optionAResult = result.Options.First(o => o.Text == "A");
        Assert.Equal(1, optionAResult.ResponseCount);
        Assert.Equal(100.0m, optionAResult.Percentage);

        var optionBResult = result.Options.First(o => o.Text == "B");
        Assert.Equal(0, optionBResult.ResponseCount);
        Assert.Equal(0m, optionBResult.Percentage);
    }

    [Fact]
    public async Task GetResults_NotResponded_Throws()
    {
        await using var context = new InMemoryDatabaseContext();
        var author = await new UserFake(Role.Interviewer).PleaseAsync(context);
        var viewer = await new UserFake(Role.Interviewer).PleaseAsync(context);
        CreatePublishedSurvey(author, context);

        context.ChangeTracker.Clear();

        await Assert.ThrowsAsync<BadRequestException>(() =>
            new GetPublicSurveyResultsHandler(context, new FakeAuth(viewer))
                .Handle("test-slug", default));
    }

    [Fact]
    public async Task GetResults_DraftSurvey_Throws()
    {
        await using var context = new InMemoryDatabaseContext();
        var author = await new UserFake(Role.Interviewer).PleaseAsync(context);

        var survey = new PublicSurvey("Test", "desc", "test-slug", author.Id);
        var question = new PublicSurveyQuestion("Q?", 0, survey);
        survey.Questions.Add(question);
        question.AddOption("A", 0);
        question.AddOption("B", 1);
        context.PublicSurveys.Add(survey);
        context.SaveChanges();

        context.ChangeTracker.Clear();

        await Assert.ThrowsAsync<NotFoundException>(() =>
            new GetPublicSurveyResultsHandler(context, new FakeAuth(author))
                .Handle("test-slug", default));
    }

    [Fact]
    public async Task GetResults_MultipleRespondents_CorrectPercentages()
    {
        await using var context = new InMemoryDatabaseContext();
        var author = await new UserFake(Role.Interviewer).PleaseAsync(context);
        var user1 = await new UserFake(Role.Interviewer).PleaseAsync(context);
        var user2 = await new UserFake(Role.Interviewer).PleaseAsync(context);
        var (survey, question) = CreatePublishedSurvey(author, context);

        var optionA = question.Options.First(o => o.Text == "A");
        var optionB = question.Options.First(o => o.Text == "B");

        await SubmitResponse(context, user1, "test-slug", new List<Guid> { optionA.Id });
        await SubmitResponse(context, user2, "test-slug", new List<Guid> { optionA.Id });
        await SubmitResponse(context, author, "test-slug", new List<Guid> { optionB.Id });

        context.ChangeTracker.Clear();

        var result = await new GetPublicSurveyResultsHandler(context, new FakeAuth(user1))
            .Handle("test-slug", default);

        Assert.Equal(3, result.TotalResponses);

        var aResult = result.Options.First(o => o.Text == "A");
        Assert.Equal(2, aResult.ResponseCount);
        Assert.Equal(66.7m, aResult.Percentage);

        var bResult = result.Options.First(o => o.Text == "B");
        Assert.Equal(1, bResult.ResponseCount);
        Assert.Equal(33.3m, bResult.Percentage);
    }

    [Fact]
    public async Task GetResults_OptionsOrderedByResponseCountDescending()
    {
        await using var context = new InMemoryDatabaseContext();
        var author = await new UserFake(Role.Interviewer).PleaseAsync(context);
        var user1 = await new UserFake(Role.Interviewer).PleaseAsync(context);
        var user2 = await new UserFake(Role.Interviewer).PleaseAsync(context);
        var (survey, question) = CreatePublishedSurvey(author, context);

        // Option A has Order=0, Option B has Order=1
        var optionA = question.Options.First(o => o.Text == "A");
        var optionB = question.Options.First(o => o.Text == "B");

        // Give B more responses than A so it should appear first
        await SubmitResponse(context, user1, "test-slug", new List<Guid> { optionB.Id });
        await SubmitResponse(context, user2, "test-slug", new List<Guid> { optionB.Id });
        await SubmitResponse(context, author, "test-slug", new List<Guid> { optionA.Id });

        context.ChangeTracker.Clear();

        var result = await new GetPublicSurveyResultsHandler(context, new FakeAuth(user1))
            .Handle("test-slug", default);

        Assert.Equal(3, result.TotalResponses);
        Assert.Equal("B", result.Options[0].Text);
        Assert.Equal(2, result.Options[0].ResponseCount);
        Assert.Equal("A", result.Options[1].Text);
        Assert.Equal(1, result.Options[1].ResponseCount);
    }

    private static async Task SubmitResponse(
        InMemoryDatabaseContext context,
        Domain.Entities.Users.User user,
        string slug,
        List<Guid> optionIds)
    {
        var command = new SubmitPublicSurveyResponseCommand(
            slug,
            new SubmitPublicSurveyResponseRequest
            {
                OptionIds = optionIds,
            });

        await new SubmitPublicSurveyResponseHandler(context, new FakeAuth(user))
            .Handle(command, default);
    }

    private static (PublicSurvey Survey, PublicSurveyQuestion Question) CreatePublishedSurvey(
        Domain.Entities.Users.User user,
        InMemoryDatabaseContext context)
    {
        var survey = new PublicSurvey("Test", "desc", "test-slug", user.Id);
        var question = new PublicSurveyQuestion("Q?", 0, survey);
        survey.Questions.Add(question);
        question.AddOption("A", 0);
        question.AddOption("B", 1);
        context.PublicSurveys.Add(survey);
        context.SaveChanges();

        survey.Publish();
        context.SaveChanges();

        return (survey, question);
    }
}
