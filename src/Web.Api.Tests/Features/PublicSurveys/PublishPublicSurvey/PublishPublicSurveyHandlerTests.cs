using System.Threading.Tasks;
using Domain.Entities.Surveys;
using Domain.Enums;
using Domain.Validation.Exceptions;
using TestUtils.Auth;
using TestUtils.Db;
using TestUtils.Fakes;
using Web.Api.Features.PublicSurveys.PublishPublicSurvey;
using Xunit;

namespace Web.Api.Tests.Features.PublicSurveys.PublishPublicSurvey;

public class PublishPublicSurveyHandlerTests
{
    [Fact]
    public async Task Publish_DraftWithValidQuestions_Ok()
    {
        await using var context = new InMemoryDatabaseContext();
        var user = await new UserFake(Role.Interviewer).PleaseAsync(context);
        var survey = CreateDraftSurvey(user, context);

        context.ChangeTracker.Clear();

        var result = await new PublishPublicSurveyHandler(context, new FakeAuth(user))
            .Handle(survey.Id, default);

        Assert.Equal(PublicSurveyStatus.Published, result.Status);
        Assert.NotNull(result.PublishedAt);
    }

    [Fact]
    public async Task Publish_AlreadyPublished_Throws()
    {
        await using var context = new InMemoryDatabaseContext();
        var user = await new UserFake(Role.Interviewer).PleaseAsync(context);
        var survey = CreateDraftSurvey(user, context);
        survey.Publish();
        context.SaveChanges();

        context.ChangeTracker.Clear();

        await Assert.ThrowsAsync<BadRequestException>(() =>
            new PublishPublicSurveyHandler(context, new FakeAuth(user))
                .Handle(survey.Id, default));
    }

    [Fact]
    public async Task Publish_NotAuthor_Throws()
    {
        await using var context = new InMemoryDatabaseContext();
        var author = await new UserFake(Role.Interviewer).PleaseAsync(context);
        var other = await new UserFake(Role.Interviewer).PleaseAsync(context);
        var survey = CreateDraftSurvey(author, context);

        context.ChangeTracker.Clear();

        await Assert.ThrowsAsync<NotFoundException>(() =>
            new PublishPublicSurveyHandler(context, new FakeAuth(other))
                .Handle(survey.Id, default));
    }

    private static PublicSurvey CreateDraftSurvey(
        Domain.Entities.Users.User user,
        InMemoryDatabaseContext context)
    {
        var survey = new PublicSurvey("Test", "desc", "test-slug", user.Id);
        var question = new PublicSurveyQuestion("Question?", 0, survey);
        survey.Questions.Add(question);
        question.AddOption("A", 0);
        question.AddOption("B", 1);
        context.PublicSurveys.Add(survey);
        context.SaveChanges();
        return survey;
    }
}
