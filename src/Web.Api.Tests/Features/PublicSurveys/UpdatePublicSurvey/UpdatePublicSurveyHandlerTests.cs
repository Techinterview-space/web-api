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
                Question = "Updated question?",
                AllowMultipleChoices = true,
            });

        var result = await new UpdatePublicSurveyHandler(context, new FakeAuth(user))
            .Handle(command, default);

        Assert.Equal("Updated Title", result.Title);
        Assert.Equal("Updated desc", result.Description);
        Assert.Equal("updated-slug", result.Slug);
        Assert.Equal("Updated question?", result.Question.Text);
        Assert.True(result.Question.AllowMultipleChoices);
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
                Options = new List<UpdatePublicSurveyOptionRequest>
                {
                    new () { Text = "New A", Order = 0 },
                    new () { Text = "New B", Order = 1 },
                    new () { Text = "New C", Order = 2 },
                },
            });

        var result = await new UpdatePublicSurveyHandler(context, new FakeAuth(user))
            .Handle(command, default);

        Assert.Equal(3, result.Question.Options.Count);
        Assert.Equal("New A", result.Question.Options[0].Text);
        Assert.Equal("New B", result.Question.Options[1].Text);
        Assert.Equal("New C", result.Question.Options[2].Text);
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
