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
using Web.Api.Features.PublicSurveys.GetPublicSurveyBySlug;
using Web.Api.Features.PublicSurveys.SubmitPublicSurveyResponse;
using Xunit;

namespace Web.Api.Tests.Features.PublicSurveys.GetPublicSurveyBySlug;

public class GetPublicSurveyBySlugHandlerTests
{
    [Fact]
    public async Task Get_PublishedSurvey_NonAuthor_Ok()
    {
        await using var context = new InMemoryDatabaseContext();
        var author = await new UserFake(Role.Interviewer).PleaseAsync(context);
        var viewer = await new UserFake(Role.Interviewer).PleaseAsync(context);
        var survey = CreatePublishedSurvey(author, context);

        context.ChangeTracker.Clear();

        var result = await new GetPublicSurveyBySlugHandler(context, new FakeAuth(viewer))
            .Handle("test-slug", default);

        Assert.NotNull(result);
        Assert.Equal("Test", result.Title);
        Assert.False(result.IsAuthor);
        Assert.NotNull(result.Questions);
        Assert.Single(result.Questions);
        Assert.False(result.Questions.First().HasUserResponded);
    }

    [Fact]
    public async Task Get_PublishedSurvey_Author_IsAuthorTrue()
    {
        await using var context = new InMemoryDatabaseContext();
        var author = await new UserFake(Role.Interviewer).PleaseAsync(context);
        var survey = CreatePublishedSurvey(author, context);

        context.ChangeTracker.Clear();

        var result = await new GetPublicSurveyBySlugHandler(context, new FakeAuth(author))
            .Handle("test-slug", default);

        Assert.True(result.IsAuthor);
    }

    [Fact]
    public async Task Get_DraftSurvey_NonAuthor_Throws()
    {
        await using var context = new InMemoryDatabaseContext();
        var author = await new UserFake(Role.Interviewer).PleaseAsync(context);
        var viewer = await new UserFake(Role.Interviewer).PleaseAsync(context);

        var survey = new PublicSurvey("Test", "desc", "test-slug", author.Id);
        var question = new PublicSurveyQuestion("Q?", 0, survey);
        survey.Questions.Add(question);
        question.AddOption("A", 0);
        question.AddOption("B", 1);
        context.PublicSurveys.Add(survey);
        context.SaveChanges();

        context.ChangeTracker.Clear();

        await Assert.ThrowsAsync<NotFoundException>(() =>
            new GetPublicSurveyBySlugHandler(context, new FakeAuth(viewer))
                .Handle("test-slug", default));
    }

    [Fact]
    public async Task Get_DraftSurvey_Author_Ok()
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

        var result = await new GetPublicSurveyBySlugHandler(context, new FakeAuth(author))
            .Handle("test-slug", default);

        Assert.NotNull(result);
        Assert.True(result.IsAuthor);
        Assert.Equal(PublicSurveyStatus.Draft, result.Status);
    }

    [Fact]
    public async Task Get_DeletedSurvey_NonAuthor_Throws()
    {
        await using var context = new InMemoryDatabaseContext();
        var author = await new UserFake(Role.Interviewer).PleaseAsync(context);
        var viewer = await new UserFake(Role.Interviewer).PleaseAsync(context);
        var survey = CreatePublishedSurvey(author, context);

        survey.Delete();
        context.SaveChanges();

        context.ChangeTracker.Clear();

        await Assert.ThrowsAsync<NotFoundException>(() =>
            new GetPublicSurveyBySlugHandler(context, new FakeAuth(viewer))
                .Handle("test-slug", default));
    }

    [Fact]
    public async Task Get_NonExistentSlug_Throws()
    {
        await using var context = new InMemoryDatabaseContext();
        var user = await new UserFake(Role.Interviewer).PleaseAsync(context);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            new GetPublicSurveyBySlugHandler(context, new FakeAuth(user))
                .Handle("non-existent", default));
    }

    [Fact]
    public async Task Get_PublishedSurvey_AfterResponding_HasUserRespondedTrue()
    {
        await using var context = new InMemoryDatabaseContext();
        var author = await new UserFake(Role.Interviewer).PleaseAsync(context);
        var respondent = await new UserFake(Role.Interviewer).PleaseAsync(context);
        var survey = CreatePublishedSurvey(author, context);
        var question = survey.Questions.First();

        // Submit a response
        var submitCommand = new SubmitPublicSurveyResponseCommand(
            "test-slug",
            new SubmitPublicSurveyResponseRequest
            {
                Answers = new List<SubmitPublicSurveyAnswerRequest>
                {
                    new () { QuestionId = question.Id, OptionIds = new List<Guid> { question.Options.First().Id } },
                },
            });

        await new SubmitPublicSurveyResponseHandler(context, new FakeAuth(respondent))
            .Handle(submitCommand, default);

        context.ChangeTracker.Clear();

        var result = await new GetPublicSurveyBySlugHandler(context, new FakeAuth(respondent))
            .Handle("test-slug", default);

        Assert.NotNull(result.Questions);
        Assert.True(result.Questions.All(q => q.HasUserResponded));
    }

    [Fact]
    public async Task Get_MultipleQuestions_HasUserRespondedOnlyIfAllAnswered()
    {
        await using var context = new InMemoryDatabaseContext();
        var author = await new UserFake(Role.Interviewer).PleaseAsync(context);
        var respondent = await new UserFake(Role.Interviewer).PleaseAsync(context);

        var survey = new PublicSurvey("Test", "desc", "test-slug", author.Id);
        var question1 = new PublicSurveyQuestion("Q1?", 0, survey);
        survey.Questions.Add(question1);
        question1.AddOption("A1", 0);
        question1.AddOption("B1", 1);

        var question2 = new PublicSurveyQuestion("Q2?", 1, survey);
        survey.Questions.Add(question2);
        question2.AddOption("A2", 0);
        question2.AddOption("B2", 1);

        context.PublicSurveys.Add(survey);
        context.SaveChanges();

        survey.Publish();
        context.SaveChanges();

        // Submit response to all questions
        var submitCommand = new SubmitPublicSurveyResponseCommand(
            "test-slug",
            new SubmitPublicSurveyResponseRequest
            {
                Answers = new List<SubmitPublicSurveyAnswerRequest>
                {
                    new () { QuestionId = question1.Id, OptionIds = new List<Guid> { question1.Options.First().Id } },
                    new () { QuestionId = question2.Id, OptionIds = new List<Guid> { question2.Options.First().Id } },
                },
            });

        await new SubmitPublicSurveyResponseHandler(context, new FakeAuth(respondent))
            .Handle(submitCommand, default);

        context.ChangeTracker.Clear();

        var result = await new GetPublicSurveyBySlugHandler(context, new FakeAuth(respondent))
            .Handle("test-slug", default);

        Assert.Equal(2, result.Questions.Count);
        Assert.True(result.Questions.All(q => q.HasUserResponded));
    }

    private static PublicSurvey CreatePublishedSurvey(
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

        return survey;
    }
}
