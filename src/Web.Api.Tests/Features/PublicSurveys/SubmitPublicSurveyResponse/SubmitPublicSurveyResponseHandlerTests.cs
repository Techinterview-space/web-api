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
                OptionIds = new List<Guid> { optionA.Id },
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
                OptionIds = question.Options.Select(o => o.Id).ToList(),
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
                OptionIds = question.Options.Select(o => o.Id).ToList(),
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
                OptionIds = new List<Guid> { optionA.Id },
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
        context.SaveChanges();

        var command = new SubmitPublicSurveyResponseCommand(
            "test-slug",
            new SubmitPublicSurveyResponseRequest
            {
                OptionIds = new List<Guid> { question.Options.First().Id },
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
        CreatePublishedSurvey(author, context);

        var command = new SubmitPublicSurveyResponseCommand(
            "test-slug",
            new SubmitPublicSurveyResponseRequest
            {
                OptionIds = new List<Guid> { Guid.NewGuid() },
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
                OptionIds = new List<Guid> { question.Options.First().Id },
            });

        context.ChangeTracker.Clear();

        await new SubmitPublicSurveyResponseHandler(context, new FakeAuth(author))
            .Handle(command, default);

        Assert.Equal(1, context.PublicSurveyResponses.Count());
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
}
