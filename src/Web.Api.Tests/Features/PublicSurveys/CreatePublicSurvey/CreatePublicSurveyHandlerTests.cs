using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Domain.Enums;
using Domain.Validation.Exceptions;
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
            Question = "What is your favorite color?",
            AllowMultipleChoices = false,
            Options = new List<string> { "Red", "Blue", "Green" },
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

        Assert.NotNull(result.Question);
        Assert.Equal("What is your favorite color?", result.Question.Text);
        Assert.False(result.Question.AllowMultipleChoices);
        Assert.Equal(3, result.Question.Options.Count);
        Assert.Equal("Red", result.Question.Options[0].Text);
        Assert.Equal("Blue", result.Question.Options[1].Text);
        Assert.Equal("Green", result.Question.Options[2].Text);

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
            Question = "Select all that apply",
            AllowMultipleChoices = true,
            Options = new List<string> { "A", "B" },
        };

        var result = await new CreatePublicSurveyHandler(context, new FakeAuth(user))
            .Handle(request, default);

        Assert.True(result.Question.AllowMultipleChoices);
        Assert.Equal(2, result.Question.Options.Count);
    }

    [Fact]
    public async Task Create_TooFewOptions_Throws()
    {
        await using var context = new InMemoryDatabaseContext();
        var user = await new UserFake(Role.Interviewer).PleaseAsync(context);

        var request = new CreatePublicSurveyRequest
        {
            Title = "Bad Survey",
            Slug = "bad-survey",
            Question = "Only one option",
            Options = new List<string> { "Only" },
        };

        await Assert.ThrowsAnyAsync<InvalidOperationException>(() =>
            new CreatePublicSurveyHandler(context, new FakeAuth(user))
                .Handle(request, default));

        Assert.Equal(0, context.PublicSurveys.Count());
    }
}
