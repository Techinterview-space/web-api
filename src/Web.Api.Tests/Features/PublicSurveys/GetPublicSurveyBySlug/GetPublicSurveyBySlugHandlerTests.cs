using System.Linq;
using System.Threading.Tasks;
using Domain.Entities.Surveys;
using Domain.Enums;
using Domain.Validation.Exceptions;
using TestUtils.Auth;
using TestUtils.Db;
using TestUtils.Fakes;
using Web.Api.Features.PublicSurveys.GetPublicSurveyBySlug;
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
        Assert.False(result.Question.HasUserResponded);
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
