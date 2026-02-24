using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Domain.Entities.Surveys;
using Domain.Enums;
using Domain.Validation.Exceptions;
using Microsoft.EntityFrameworkCore;
using TestUtils.Auth;
using TestUtils.Db;
using TestUtils.Fakes;
using Web.Api.Features.PublicSurveys.CreatePublicSurvey;
using Xunit;

namespace Web.Api.Tests.Features.PublicSurveys.CreatePublicSurvey;

public class CreatePublicSurveyHandlerTests
{
    [Fact]
    public async Task Create_ValidData_Ok()
    {
        await using var context = new InMemoryDatabaseContext();
        var user = await new UserFake(Role.Interviewer).PleaseAsync(context);

        var request = new CreatePublicSurveyRequest
        {
            Title = "Test Survey",
            Description = "A test survey",
            Slug = "test-survey",
            Questions = new List<CreatePublicSurveyQuestionRequest>
            {
                new ()
                {
                    Text = "What is your favorite color?",
                    Order = 0,
                    AllowMultipleChoices = false,
                    Options = new List<string> { "Red", "Blue", "Green" },
                },
            },
        };

        var result = await new CreatePublicSurveyHandler(context, new FakeAuth(user))
            .Handle(request, default);

        Assert.NotNull(result);
        Assert.Equal("Test Survey", result.Title);
        Assert.Equal("A test survey", result.Description);
        Assert.Equal("test-survey", result.Slug);
        Assert.Equal(PublicSurveyStatus.Draft, result.Status);
        Assert.True(result.IsAuthor);
        Assert.Null(result.PublishedAt);

        Assert.NotNull(result.Questions);
        Assert.Single(result.Questions);
        Assert.Equal("What is your favorite color?", result.Questions[0].Text);
        Assert.False(result.Questions[0].AllowMultipleChoices);
        Assert.Equal(3, result.Questions[0].Options.Count);
        Assert.Equal("Red", result.Questions[0].Options[0].Text);
        Assert.Equal("Blue", result.Questions[0].Options[1].Text);
        Assert.Equal("Green", result.Questions[0].Options[2].Text);

        Assert.Equal(1, context.PublicSurveys.Count());
        Assert.Equal(1, context.PublicSurveyQuestions.Count());
        Assert.Equal(3, context.PublicSurveyOptions.Count());
    }

    [Fact]
    public async Task Create_WithMultipleChoices_Ok()
    {
        await using var context = new InMemoryDatabaseContext();
        var user = await new UserFake(Role.Interviewer).PleaseAsync(context);

        var request = new CreatePublicSurveyRequest
        {
            Title = "Multi Survey",
            Slug = "multi-survey",
            Questions = new List<CreatePublicSurveyQuestionRequest>
            {
                new ()
                {
                    Text = "Select all that apply",
                    Order = 0,
                    AllowMultipleChoices = true,
                    Options = new List<string> { "A", "B" },
                },
            },
        };

        var result = await new CreatePublicSurveyHandler(context, new FakeAuth(user))
            .Handle(request, default);

        Assert.True(result.Questions[0].AllowMultipleChoices);
        Assert.Equal(2, result.Questions[0].Options.Count);
    }

    [Fact]
    public async Task Create_TooFewOptions_CreatesAsDraft_ButCannotPublish()
    {
        await using var context = new InMemoryDatabaseContext();
        var user = await new UserFake(Role.Interviewer).PleaseAsync(context);

        var request = new CreatePublicSurveyRequest
        {
            Title = "Bad Survey",
            Slug = "bad-survey",
            Questions = new List<CreatePublicSurveyQuestionRequest>
            {
                new ()
                {
                    Text = "Only one option",
                    Order = 0,
                    Options = new List<string> { "Only" },
                },
            },
        };

        var result = await new CreatePublicSurveyHandler(context, new FakeAuth(user))
            .Handle(request, default);

        Assert.Equal(1, context.PublicSurveys.Count());
        Assert.Equal(PublicSurveyStatus.Draft, result.Status);
        Assert.Single(result.Questions[0].Options);

        // Attempting to publish should fail because question has fewer than 2 options
        var survey = context.PublicSurveys
            .Include(s => s.Questions)
            .ThenInclude(q => q.Options)
            .First();
        Assert.Throws<BadRequestException>(() => survey.Publish());
    }

    [Fact]
    public async Task Create_MultipleQuestions_Ok()
    {
        await using var context = new InMemoryDatabaseContext();
        var user = await new UserFake(Role.Interviewer).PleaseAsync(context);

        var request = new CreatePublicSurveyRequest
        {
            Title = "Multi-Question Survey",
            Slug = "multi-question-survey",
            Questions = new List<CreatePublicSurveyQuestionRequest>
            {
                new ()
                {
                    Text = "Question 1?",
                    Order = 0,
                    AllowMultipleChoices = false,
                    Options = new List<string> { "A1", "B1" },
                },
                new ()
                {
                    Text = "Question 2?",
                    Order = 1,
                    AllowMultipleChoices = true,
                    Options = new List<string> { "A2", "B2", "C2" },
                },
            },
        };

        var result = await new CreatePublicSurveyHandler(context, new FakeAuth(user))
            .Handle(request, default);

        Assert.NotNull(result);
        Assert.Equal(2, result.Questions.Count);

        Assert.Equal("Question 1?", result.Questions[0].Text);
        Assert.Equal(0, result.Questions[0].Order);
        Assert.False(result.Questions[0].AllowMultipleChoices);
        Assert.Equal(2, result.Questions[0].Options.Count);

        Assert.Equal("Question 2?", result.Questions[1].Text);
        Assert.Equal(1, result.Questions[1].Order);
        Assert.True(result.Questions[1].AllowMultipleChoices);
        Assert.Equal(3, result.Questions[1].Options.Count);

        Assert.Equal(1, context.PublicSurveys.Count());
        Assert.Equal(2, context.PublicSurveyQuestions.Count());
        Assert.Equal(5, context.PublicSurveyOptions.Count());
    }
}
