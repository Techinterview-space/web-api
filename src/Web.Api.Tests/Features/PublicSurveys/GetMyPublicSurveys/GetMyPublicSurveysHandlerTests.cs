using System.Linq;
using System.Threading.Tasks;
using Domain.Entities.Surveys;
using Domain.Enums;
using TestUtils.Auth;
using TestUtils.Db;
using TestUtils.Fakes;
using Web.Api.Features.PublicSurveys.GetMyPublicSurveys;
using Xunit;

namespace Web.Api.Tests.Features.PublicSurveys.GetMyPublicSurveys;

public class GetMyPublicSurveysHandlerTests
{
    [Fact]
    public async Task GetMy_ReturnsOnlyOwnSurveys()
    {
        await using var context = new InMemoryDatabaseContext();
        var user1 = await new UserFake(Role.Interviewer).PleaseAsync(context);
        var user2 = await new UserFake(Role.Interviewer).PleaseAsync(context);

        context.PublicSurveys.Add(new PublicSurvey("Survey 1", null, "slug-1", user1.Id));
        context.PublicSurveys.Add(new PublicSurvey("Survey 2", null, "slug-2", user1.Id));
        context.PublicSurveys.Add(new PublicSurvey("Other", null, "slug-3", user2.Id));
        context.SaveChanges();

        context.ChangeTracker.Clear();

        var result = await new GetMyPublicSurveysHandler(context, new FakeAuth(user1))
            .Handle(new GetMyPublicSurveysQuery(), default);

        Assert.Equal(2, result.Count);
        Assert.All(result, s => Assert.Contains("Survey", s.Title));
    }

    [Fact]
    public async Task GetMy_ExcludesDeletedByDefault()
    {
        await using var context = new InMemoryDatabaseContext();
        var user = await new UserFake(Role.Interviewer).PleaseAsync(context);

        var survey1 = new PublicSurvey("Active", null, "slug-1", user.Id);
        var survey2 = new PublicSurvey("Deleted", null, "slug-2", user.Id);
        context.PublicSurveys.Add(survey1);
        context.PublicSurveys.Add(survey2);
        context.SaveChanges();

        survey2.Delete();
        context.SaveChanges();

        context.ChangeTracker.Clear();

        var result = await new GetMyPublicSurveysHandler(context, new FakeAuth(user))
            .Handle(new GetMyPublicSurveysQuery { IncludeDeleted = false }, default);

        Assert.Single(result);
        Assert.Equal("Active", result.First().Title);
    }

    [Fact]
    public async Task GetMy_IncludesDeletedWhenRequested()
    {
        await using var context = new InMemoryDatabaseContext();
        var user = await new UserFake(Role.Interviewer).PleaseAsync(context);

        var survey1 = new PublicSurvey("Active", null, "slug-1", user.Id);
        var survey2 = new PublicSurvey("Deleted", null, "slug-2", user.Id);
        context.PublicSurveys.Add(survey1);
        context.PublicSurveys.Add(survey2);
        context.SaveChanges();

        survey2.Delete();
        context.SaveChanges();

        context.ChangeTracker.Clear();

        var result = await new GetMyPublicSurveysHandler(context, new FakeAuth(user))
            .Handle(new GetMyPublicSurveysQuery { IncludeDeleted = true }, default);

        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task GetMy_FilterByStatus()
    {
        await using var context = new InMemoryDatabaseContext();
        var user = await new UserFake(Role.Interviewer).PleaseAsync(context);

        var draft = new PublicSurvey("Draft", null, "slug-1", user.Id);
        context.PublicSurveys.Add(draft);

        var published = new PublicSurvey("Published", null, "slug-2", user.Id);
        var question = new PublicSurveyQuestion("Q?", 0, published);
        published.Questions.Add(question);
        question.AddOption("A", 0);
        question.AddOption("B", 1);
        context.PublicSurveys.Add(published);
        context.SaveChanges();

        published.Publish();
        context.SaveChanges();

        context.ChangeTracker.Clear();

        var result = await new GetMyPublicSurveysHandler(context, new FakeAuth(user))
            .Handle(new GetMyPublicSurveysQuery { Status = PublicSurveyStatus.Published }, default);

        Assert.Single(result);
        Assert.Equal("Published", result.First().Title);
    }
}
