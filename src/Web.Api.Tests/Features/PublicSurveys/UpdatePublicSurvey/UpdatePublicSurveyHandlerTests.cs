using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Domain.Entities.Surveys;
using Domain.Enums;
using Domain.Validation.Exceptions;
using Infrastructure.Database;
using TestUtils.Auth;
using TestUtils.Db;
using TestUtils.Fakes;
using Web.Api.Features.PublicSurveys.UpdatePublicSurvey;
using Xunit;

namespace Web.Api.Tests.Features.PublicSurveys.UpdatePublicSurvey;

public class UpdatePublicSurveyHandlerTests
{
    [Fact]
    public async Task Update_DraftSurvey_TextFields_Ok()
    {
        await using var context = new InMemoryDatabaseContext();
        var user = await new UserFake(Role.Interviewer).PleaseAsync(context);
        var survey = CreateDraftSurvey(user, context);

        var command = new UpdatePublicSurveyCommand(
            survey.Id,
            new UpdatePublicSurveyRequest
            {
                Title = "Updated Title",
                Description = "Updated desc",
                Slug = "updated-slug",
                Questions = new List<UpdatePublicSurveyQuestionRequest>
                {
                    new ()
                    {
                        Text = "Updated question?",
                        Order = 0,
                        AllowMultipleChoices = true,
                        Options = new List<UpdatePublicSurveyOptionRequest>
                        {
                            new () { Text = "Option A", Order = 0 },
                            new () { Text = "Option B", Order = 1 },
                        },
                    },
                },
            });

        var result = await new UpdatePublicSurveyHandler(context, new FakeAuth(user))
            .Handle(command, default);

        Assert.Equal("Updated Title", result.Title);
        Assert.Equal("Updated desc", result.Description);
        Assert.Equal("updated-slug", result.Slug);
        Assert.Single(result.Questions);
        Assert.Equal("Updated question?", result.Questions[0].Text);
        Assert.True(result.Questions[0].AllowMultipleChoices);
    }

    [Fact]
    public async Task Update_DraftSurvey_ReplaceOptions_Ok()
    {
        await using var context = new SqliteContext();
        var user = await new UserFake(Role.Interviewer).PleaseAsync(context);
        var survey = CreateDraftSurvey(user, context);

        context.ChangeTracker.Clear();

        var command = new UpdatePublicSurveyCommand(
            survey.Id,
            new UpdatePublicSurveyRequest
            {
                Questions = new List<UpdatePublicSurveyQuestionRequest>
                {
                    new ()
                    {
                        Text = "Replaced question?",
                        Order = 0,
                        AllowMultipleChoices = false,
                        Options = new List<UpdatePublicSurveyOptionRequest>
                        {
                            new () { Text = "New A", Order = 0 },
                            new () { Text = "New B", Order = 1 },
                            new () { Text = "New C", Order = 2 },
                        },
                    },
                },
            });

        var result = await new UpdatePublicSurveyHandler(context, new FakeAuth(user))
            .Handle(command, default);

        Assert.Single(result.Questions);
        Assert.Equal(3, result.Questions[0].Options.Count);
        Assert.Equal("New A", result.Questions[0].Options[0].Text);
        Assert.Equal("New B", result.Questions[0].Options[1].Text);
        Assert.Equal("New C", result.Questions[0].Options[2].Text);
    }

    [Fact]
    public async Task Update_PublishedSurvey_DescriptionOnly_Ok()
    {
        await using var context = new InMemoryDatabaseContext();
        var user = await new UserFake(Role.Interviewer).PleaseAsync(context);
        var survey = CreatePublishedSurvey(user, context);

        var command = new UpdatePublicSurveyCommand(
            survey.Id,
            new UpdatePublicSurveyRequest
            {
                Description = "New description",
            });

        context.ChangeTracker.Clear();

        var result = await new UpdatePublicSurveyHandler(context, new FakeAuth(user))
            .Handle(command, default);

        Assert.Equal("New description", result.Description);
    }

    [Fact]
    public async Task Update_PublishedSurvey_TitleChange_Throws()
    {
        await using var context = new InMemoryDatabaseContext();
        var user = await new UserFake(Role.Interviewer).PleaseAsync(context);
        var survey = CreatePublishedSurvey(user, context);

        var command = new UpdatePublicSurveyCommand(
            survey.Id,
            new UpdatePublicSurveyRequest
            {
                Title = "New Title",
            });

        context.ChangeTracker.Clear();

        await Assert.ThrowsAsync<BadRequestException>(() =>
            new UpdatePublicSurveyHandler(context, new FakeAuth(user))
                .Handle(command, default));
    }

    [Fact]
    public async Task Update_PublishedSurvey_QuestionsChange_Throws()
    {
        await using var context = new InMemoryDatabaseContext();
        var user = await new UserFake(Role.Interviewer).PleaseAsync(context);
        var survey = CreatePublishedSurvey(user, context);

        var command = new UpdatePublicSurveyCommand(
            survey.Id,
            new UpdatePublicSurveyRequest
            {
                Questions = new List<UpdatePublicSurveyQuestionRequest>
                {
                    new ()
                    {
                        Text = "New question?",
                        Order = 0,
                        Options = new List<UpdatePublicSurveyOptionRequest>
                        {
                            new () { Text = "X", Order = 0 },
                            new () { Text = "Y", Order = 1 },
                        },
                    },
                },
            });

        context.ChangeTracker.Clear();

        await Assert.ThrowsAsync<BadRequestException>(() =>
            new UpdatePublicSurveyHandler(context, new FakeAuth(user))
                .Handle(command, default));
    }

    [Fact]
    public async Task Update_NotAuthor_Throws()
    {
        await using var context = new InMemoryDatabaseContext();
        var author = await new UserFake(Role.Interviewer).PleaseAsync(context);
        var otherUser = await new UserFake(Role.Interviewer).PleaseAsync(context);
        var survey = CreateDraftSurvey(author, context);

        var command = new UpdatePublicSurveyCommand(
            survey.Id,
            new UpdatePublicSurveyRequest
            {
                Description = "Hacked",
            });

        context.ChangeTracker.Clear();

        await Assert.ThrowsAsync<NotFoundException>(() =>
            new UpdatePublicSurveyHandler(context, new FakeAuth(otherUser))
                .Handle(command, default));
    }

    [Fact]
    public async Task Update_DraftSurvey_MultipleQuestions_Ok()
    {
        await using var context = new SqliteContext();
        var user = await new UserFake(Role.Interviewer).PleaseAsync(context);
        var survey = CreateDraftSurvey(user, context);

        context.ChangeTracker.Clear();

        var command = new UpdatePublicSurveyCommand(
            survey.Id,
            new UpdatePublicSurveyRequest
            {
                Questions = new List<UpdatePublicSurveyQuestionRequest>
                {
                    new ()
                    {
                        Text = "First question?",
                        Order = 0,
                        AllowMultipleChoices = false,
                        Options = new List<UpdatePublicSurveyOptionRequest>
                        {
                            new () { Text = "A1", Order = 0 },
                            new () { Text = "B1", Order = 1 },
                        },
                    },
                    new ()
                    {
                        Text = "Second question?",
                        Order = 1,
                        AllowMultipleChoices = true,
                        Options = new List<UpdatePublicSurveyOptionRequest>
                        {
                            new () { Text = "A2", Order = 0 },
                            new () { Text = "B2", Order = 1 },
                            new () { Text = "C2", Order = 2 },
                        },
                    },
                },
            });

        var result = await new UpdatePublicSurveyHandler(context, new FakeAuth(user))
            .Handle(command, default);

        Assert.Equal(2, result.Questions.Count);
        Assert.Equal("First question?", result.Questions[0].Text);
        Assert.Equal(2, result.Questions[0].Options.Count);
        Assert.Equal("Second question?", result.Questions[1].Text);
        Assert.Equal(3, result.Questions[1].Options.Count);
        Assert.True(result.Questions[1].AllowMultipleChoices);
    }

    [Fact]
    public async Task Update_DraftSurvey_ExceedsMaxQuestions_Throws()
    {
        await using var context = new InMemoryDatabaseContext();
        var user = await new UserFake(Role.Interviewer).PleaseAsync(context);
        var survey = CreateDraftSurvey(user, context);

        var questions = new List<UpdatePublicSurveyQuestionRequest>();
        for (int i = 0; i < PublicSurvey.MaxQuestions + 1; i++)
        {
            questions.Add(new UpdatePublicSurveyQuestionRequest
            {
                Text = $"Question {i}?",
                Order = i,
                AllowMultipleChoices = false,
                Options = new List<UpdatePublicSurveyOptionRequest>
                {
                    new () { Text = "A", Order = 0 },
                    new () { Text = "B", Order = 1 },
                },
            });
        }

        var command = new UpdatePublicSurveyCommand(
            survey.Id,
            new UpdatePublicSurveyRequest
            {
                Questions = questions,
            });

        context.ChangeTracker.Clear();

        // MaxLength(30) attribute on the request triggers EntityInvalidException via ThrowIfInvalid
        await Assert.ThrowsAsync<EntityInvalidException>(() =>
            new UpdatePublicSurveyHandler(context, new FakeAuth(user))
                .Handle(command, default));
    }

    [Fact]
    public async Task Update_DraftSurvey_ExceedsMaxQuestions_DomainValidation_Throws()
    {
        await using var context = new InMemoryDatabaseContext();
        var user = await new UserFake(Role.Interviewer).PleaseAsync(context);
        var survey = CreateDraftSurvey(user, context);

        // Verify that the domain entity itself also rejects > MaxQuestions
        Assert.Throws<BadRequestException>(() =>
        {
            for (int i = 0; i < PublicSurvey.MaxQuestions + 1; i++)
            {
                survey.AddQuestion($"Q{i}?", i);
            }
        });
    }

    private static PublicSurvey CreateDraftSurvey(
        Domain.Entities.Users.User user,
        DatabaseContext context)
    {
        var survey = new PublicSurvey("Test Survey", "desc", "test-slug", user.Id);
        var question = new PublicSurveyQuestion("Question?", 0, survey);
        survey.Questions.Add(question);
        question.AddOption("Option A", 0);
        question.AddOption("Option B", 1);
        context.PublicSurveys.Add(survey);
        context.SaveChanges();
        return survey;
    }

    private static PublicSurvey CreatePublishedSurvey(
        Domain.Entities.Users.User user,
        InMemoryDatabaseContext context)
    {
        var survey = CreateDraftSurvey(user, context);
        survey.Publish();
        context.SaveChanges();
        return survey;
    }
}
