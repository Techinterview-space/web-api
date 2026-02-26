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
using Web.Api.Features.PublicSurveys.SubmitPublicSurveyResponse;
using Xunit;

namespace Web.Api.Tests.Features.PublicSurveys.SubmitPublicSurveyResponse;

public class SubmitPublicSurveyResponseHandlerTests
{
    [Fact]
    public async Task Submit_ValidSingleChoice_Ok()
    {
        await using var context = new InMemoryDatabaseContext();
        var author = await new UserFake(Role.Interviewer).PleaseAsync(context);
        var respondent = await new UserFake(Role.Interviewer).PleaseAsync(context);
        var (survey, question) = CreatePublishedSurvey(author, context);

        var optionA = question.Options.First(o => o.Text == "A");

        var command = new SubmitPublicSurveyResponseCommand(
            "test-slug",
            new SubmitPublicSurveyResponseRequest
            {
                Answers = new List<SubmitPublicSurveyAnswerRequest>
                {
                    new () { QuestionId = question.Id, OptionIds = new List<Guid> { optionA.Id } },
                },
            });

        context.ChangeTracker.Clear();

        await new SubmitPublicSurveyResponseHandler(context, new FakeAuth(respondent))
            .Handle(command, default);

        Assert.Equal(1, context.PublicSurveyResponses.Count());
        Assert.Equal(1, context.PublicSurveyResponseOptions.Count());
    }

    [Fact]
    public async Task Submit_MultipleChoices_Ok()
    {
        await using var context = new InMemoryDatabaseContext();
        var author = await new UserFake(Role.Interviewer).PleaseAsync(context);
        var respondent = await new UserFake(Role.Interviewer).PleaseAsync(context);
        var (survey, question) = CreatePublishedSurvey(author, context, allowMultiple: true);

        var command = new SubmitPublicSurveyResponseCommand(
            "test-slug",
            new SubmitPublicSurveyResponseRequest
            {
                Answers = new List<SubmitPublicSurveyAnswerRequest>
                {
                    new ()
                    {
                        QuestionId = question.Id,
                        OptionIds = question.Options.Select(o => o.Id).ToList(),
                    },
                },
            });

        context.ChangeTracker.Clear();

        await new SubmitPublicSurveyResponseHandler(context, new FakeAuth(respondent))
            .Handle(command, default);

        Assert.Equal(1, context.PublicSurveyResponses.Count());
        Assert.Equal(2, context.PublicSurveyResponseOptions.Count());
    }

    [Fact]
    public async Task Submit_MultipleChoicesNotAllowed_Throws()
    {
        await using var context = new InMemoryDatabaseContext();
        var author = await new UserFake(Role.Interviewer).PleaseAsync(context);
        var respondent = await new UserFake(Role.Interviewer).PleaseAsync(context);
        var (survey, question) = CreatePublishedSurvey(author, context);

        var command = new SubmitPublicSurveyResponseCommand(
            "test-slug",
            new SubmitPublicSurveyResponseRequest
            {
                Answers = new List<SubmitPublicSurveyAnswerRequest>
                {
                    new ()
                    {
                        QuestionId = question.Id,
                        OptionIds = question.Options.Select(o => o.Id).ToList(),
                    },
                },
            });

        context.ChangeTracker.Clear();

        await Assert.ThrowsAsync<BadRequestException>(() =>
            new SubmitPublicSurveyResponseHandler(context, new FakeAuth(respondent))
                .Handle(command, default));
    }

    [Fact]
    public async Task Submit_DuplicateResponse_Throws()
    {
        await using var context = new InMemoryDatabaseContext();
        var author = await new UserFake(Role.Interviewer).PleaseAsync(context);
        var respondent = await new UserFake(Role.Interviewer).PleaseAsync(context);
        var (survey, question) = CreatePublishedSurvey(author, context);

        var optionA = question.Options.First();

        var command = new SubmitPublicSurveyResponseCommand(
            "test-slug",
            new SubmitPublicSurveyResponseRequest
            {
                Answers = new List<SubmitPublicSurveyAnswerRequest>
                {
                    new () { QuestionId = question.Id, OptionIds = new List<Guid> { optionA.Id } },
                },
            });

        context.ChangeTracker.Clear();

        await new SubmitPublicSurveyResponseHandler(context, new FakeAuth(respondent))
            .Handle(command, default);

        context.ChangeTracker.Clear();

        await Assert.ThrowsAsync<BadRequestException>(() =>
            new SubmitPublicSurveyResponseHandler(context, new FakeAuth(respondent))
                .Handle(command, default));
    }

    [Fact]
    public async Task Submit_ClosedSurvey_Throws()
    {
        await using var context = new InMemoryDatabaseContext();
        var author = await new UserFake(Role.Interviewer).PleaseAsync(context);
        var respondent = await new UserFake(Role.Interviewer).PleaseAsync(context);
        var (survey, question) = CreatePublishedSurvey(author, context);

        survey.Close();
        await context.SaveChangesAsync();

        var command = new SubmitPublicSurveyResponseCommand(
            "test-slug",
            new SubmitPublicSurveyResponseRequest
            {
                Answers = new List<SubmitPublicSurveyAnswerRequest>
                {
                    new () { QuestionId = question.Id, OptionIds = new List<Guid> { question.Options.First().Id } },
                },
            });

        context.ChangeTracker.Clear();

        await Assert.ThrowsAsync<BadRequestException>(() =>
            new SubmitPublicSurveyResponseHandler(context, new FakeAuth(respondent))
                .Handle(command, default));
    }

    [Fact]
    public async Task Submit_InvalidOptionId_Throws()
    {
        await using var context = new InMemoryDatabaseContext();
        var author = await new UserFake(Role.Interviewer).PleaseAsync(context);
        var respondent = await new UserFake(Role.Interviewer).PleaseAsync(context);
        var (survey, question) = CreatePublishedSurvey(author, context);

        var command = new SubmitPublicSurveyResponseCommand(
            "test-slug",
            new SubmitPublicSurveyResponseRequest
            {
                Answers = new List<SubmitPublicSurveyAnswerRequest>
                {
                    new () { QuestionId = question.Id, OptionIds = new List<Guid> { Guid.NewGuid() } },
                },
            });

        context.ChangeTracker.Clear();

        await Assert.ThrowsAsync<BadRequestException>(() =>
            new SubmitPublicSurveyResponseHandler(context, new FakeAuth(respondent))
                .Handle(command, default));
    }

    [Fact]
    public async Task Submit_AuthorCanRespondToo_Ok()
    {
        await using var context = new InMemoryDatabaseContext();
        var author = await new UserFake(Role.Interviewer).PleaseAsync(context);
        var (survey, question) = CreatePublishedSurvey(author, context);

        var command = new SubmitPublicSurveyResponseCommand(
            "test-slug",
            new SubmitPublicSurveyResponseRequest
            {
                Answers = new List<SubmitPublicSurveyAnswerRequest>
                {
                    new () { QuestionId = question.Id, OptionIds = new List<Guid> { question.Options.First().Id } },
                },
            });

        context.ChangeTracker.Clear();

        await new SubmitPublicSurveyResponseHandler(context, new FakeAuth(author))
            .Handle(command, default);

        Assert.Equal(1, context.PublicSurveyResponses.Count());
    }

    [Fact]
    public async Task Submit_MultipleQuestions_Ok()
    {
        await using var context = new InMemoryDatabaseContext();
        var author = await new UserFake(Role.Interviewer).PleaseAsync(context);
        var respondent = await new UserFake(Role.Interviewer).PleaseAsync(context);
        var (survey, question1, question2) = CreatePublishedSurveyWithTwoQuestions(author, context);

        var command = new SubmitPublicSurveyResponseCommand(
            "test-slug",
            new SubmitPublicSurveyResponseRequest
            {
                Answers = new List<SubmitPublicSurveyAnswerRequest>
                {
                    new () { QuestionId = question1.Id, OptionIds = new List<Guid> { question1.Options.First().Id } },
                    new () { QuestionId = question2.Id, OptionIds = new List<Guid> { question2.Options.First().Id } },
                },
            });

        context.ChangeTracker.Clear();

        await new SubmitPublicSurveyResponseHandler(context, new FakeAuth(respondent))
            .Handle(command, default);

        Assert.Equal(2, context.PublicSurveyResponses.Count());
        Assert.Equal(2, context.PublicSurveyResponseOptions.Count());
    }

    [Fact]
    public async Task Submit_DuplicateQuestionIds_Throws()
    {
        await using var context = new InMemoryDatabaseContext();
        var author = await new UserFake(Role.Interviewer).PleaseAsync(context);
        var respondent = await new UserFake(Role.Interviewer).PleaseAsync(context);
        var (survey, question) = CreatePublishedSurvey(author, context);

        var optionA = question.Options.First();
        var optionB = question.Options.Last();

        // Submit two answers for the same question
        var command = new SubmitPublicSurveyResponseCommand(
            "test-slug",
            new SubmitPublicSurveyResponseRequest
            {
                Answers = new List<SubmitPublicSurveyAnswerRequest>
                {
                    new () { QuestionId = question.Id, OptionIds = new List<Guid> { optionA.Id } },
                    new () { QuestionId = question.Id, OptionIds = new List<Guid> { optionB.Id } },
                },
            });

        context.ChangeTracker.Clear();

        await Assert.ThrowsAsync<BadRequestException>(() =>
            new SubmitPublicSurveyResponseHandler(context, new FakeAuth(respondent))
                .Handle(command, default));
    }

    [Fact]
    public async Task Submit_MultipleQuestions_MissingAnswer_Throws()
    {
        await using var context = new InMemoryDatabaseContext();
        var author = await new UserFake(Role.Interviewer).PleaseAsync(context);
        var respondent = await new UserFake(Role.Interviewer).PleaseAsync(context);
        var (survey, question1, question2) = CreatePublishedSurveyWithTwoQuestions(author, context);

        // Only answer one of two questions
        var command = new SubmitPublicSurveyResponseCommand(
            "test-slug",
            new SubmitPublicSurveyResponseRequest
            {
                Answers = new List<SubmitPublicSurveyAnswerRequest>
                {
                    new () { QuestionId = question1.Id, OptionIds = new List<Guid> { question1.Options.First().Id } },
                },
            });

        context.ChangeTracker.Clear();

        await Assert.ThrowsAsync<BadRequestException>(() =>
            new SubmitPublicSurveyResponseHandler(context, new FakeAuth(respondent))
                .Handle(command, default));
    }

    private static (PublicSurvey Survey, PublicSurveyQuestion Question) CreatePublishedSurvey(
        Domain.Entities.Users.User user,
        InMemoryDatabaseContext context,
        bool allowMultiple = false)
    {
        var survey = new PublicSurvey("Test", "desc", "test-slug", user.Id);
        var question = new PublicSurveyQuestion("Q?", 0, survey, allowMultiple);
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
