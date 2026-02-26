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

        await SubmitResponse(context, respondent, "test-slug", question, new List<Guid> { optionA.Id });
        context.ChangeTracker.Clear();

        var result = await new GetPublicSurveyResultsHandler(context, new FakeAuth(respondent))
            .Handle("test-slug", default);

        Assert.Equal(1, result.TotalResponses);
        Assert.Single(result.Questions);
        Assert.Equal(2, result.Questions[0].Options.Count);

        var optionAResult = result.Questions[0].Options.First(o => o.Text == "A");
        Assert.Equal(1, optionAResult.ResponseCount);
        Assert.Equal(100.0m, optionAResult.Percentage);

        var optionBResult = result.Questions[0].Options.First(o => o.Text == "B");
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
        await context.SaveChangesAsync();

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

        await SubmitResponse(context, user1, "test-slug", question, new List<Guid> { optionA.Id });
        await SubmitResponse(context, user2, "test-slug", question, new List<Guid> { optionA.Id });
        await SubmitResponse(context, author, "test-slug", question, new List<Guid> { optionB.Id });

        context.ChangeTracker.Clear();

        var result = await new GetPublicSurveyResultsHandler(context, new FakeAuth(user1))
            .Handle("test-slug", default);

        Assert.Equal(3, result.TotalResponses);

        var aResult = result.Questions[0].Options.First(o => o.Text == "A");
        Assert.Equal(2, aResult.ResponseCount);
        Assert.Equal(66.7m, aResult.Percentage);

        var bResult = result.Questions[0].Options.First(o => o.Text == "B");
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
        await SubmitResponse(context, user1, "test-slug", question, new List<Guid> { optionB.Id });
        await SubmitResponse(context, user2, "test-slug", question, new List<Guid> { optionB.Id });
        await SubmitResponse(context, author, "test-slug", question, new List<Guid> { optionA.Id });

        context.ChangeTracker.Clear();

        var result = await new GetPublicSurveyResultsHandler(context, new FakeAuth(user1))
            .Handle("test-slug", default);

        Assert.Equal(3, result.TotalResponses);
        Assert.Equal("B", result.Questions[0].Options[0].Text);
        Assert.Equal(2, result.Questions[0].Options[0].ResponseCount);
        Assert.Equal("A", result.Questions[0].Options[1].Text);
        Assert.Equal(1, result.Questions[0].Options[1].ResponseCount);
    }

    [Fact]
    public async Task GetResults_MultipleQuestions_Ok()
    {
        await using var context = new InMemoryDatabaseContext();
        var author = await new UserFake(Role.Interviewer).PleaseAsync(context);
        var respondent = await new UserFake(Role.Interviewer).PleaseAsync(context);
        var (survey, question1, question2) = CreatePublishedSurveyWithTwoQuestions(author, context);

        var q1OptionA = question1.Options.First(o => o.Text == "A1");
        var q2OptionB = question2.Options.First(o => o.Text == "B2");

        await SubmitMultiQuestionResponse(
            context,
            respondent,
            "test-slug",
            new List<SubmitPublicSurveyAnswerRequest>
            {
                new () { QuestionId = question1.Id, OptionIds = new List<Guid> { q1OptionA.Id } },
                new () { QuestionId = question2.Id, OptionIds = new List<Guid> { q2OptionB.Id } },
            });

        context.ChangeTracker.Clear();

        var result = await new GetPublicSurveyResultsHandler(context, new FakeAuth(respondent))
            .Handle("test-slug", default);

        Assert.Equal(1, result.TotalResponses);
        Assert.Equal(2, result.Questions.Count);

        var q1Result = result.Questions.First(q => q.Text == "Q1?");
        Assert.Equal(2, q1Result.Options.Count);
        var q1AResult = q1Result.Options.First(o => o.Text == "A1");
        Assert.Equal(1, q1AResult.ResponseCount);

        var q2Result = result.Questions.First(q => q.Text == "Q2?");
        Assert.Equal(2, q2Result.Options.Count);
        var q2BResult = q2Result.Options.First(o => o.Text == "B2");
        Assert.Equal(1, q2BResult.ResponseCount);
    }

    private static async Task SubmitResponse(
        InMemoryDatabaseContext context,
        Domain.Entities.Users.User user,
        string slug,
        PublicSurveyQuestion question,
        List<Guid> optionIds)
    {
        var command = new SubmitPublicSurveyResponseCommand(
            slug,
            new SubmitPublicSurveyResponseRequest
            {
                Answers = new List<SubmitPublicSurveyAnswerRequest>
                {
                    new () { QuestionId = question.Id, OptionIds = optionIds },
                },
            });

        await new SubmitPublicSurveyResponseHandler(context, new FakeAuth(user))
            .Handle(command, default);
    }

    private static async Task SubmitMultiQuestionResponse(
        InMemoryDatabaseContext context,
        Domain.Entities.Users.User user,
        string slug,
        List<SubmitPublicSurveyAnswerRequest> answers)
    {
        var command = new SubmitPublicSurveyResponseCommand(
            slug,
            new SubmitPublicSurveyResponseRequest
            {
                Answers = answers,
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

    private static (PublicSurvey Survey, PublicSurveyQuestion Question1, PublicSurveyQuestion Question2) CreatePublishedSurveyWithTwoQuestions(
        Domain.Entities.Users.User user,
        InMemoryDatabaseContext context)
    {
        var survey = new PublicSurvey("Test", "desc", "test-slug", user.Id);
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

        return (survey, question1, question2);
    }
}
