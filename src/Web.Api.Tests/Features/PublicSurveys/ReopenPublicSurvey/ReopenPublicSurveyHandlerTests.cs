using System.Threading.Tasks;
using Domain.Entities.Surveys;
using Domain.Enums;
using Domain.Validation.Exceptions;
using TestUtils.Auth;
using TestUtils.Db;
using TestUtils.Fakes;
using Web.Api.Features.PublicSurveys.ReopenPublicSurvey;
using Xunit;

namespace Web.Api.Tests.Features.PublicSurveys.ReopenPublicSurvey;

public class ReopenPublicSurveyHandlerTests
{
    [Fact]
    public async Task Reopen_Closed_Ok()
    {
        await using var context = new InMemoryDatabaseContext();
        var user = await new UserFake(Role.Interviewer).PleaseAsync(context);
        var survey = CreateClosedSurvey(user, context);

        context.ChangeTracker.Clear();

        var result = await new ReopenPublicSurveyHandler(context, new FakeAuth(user))
            .Handle(survey.Id, default);

        Assert.Equal(PublicSurveyStatus.Published, result.Status);
    }

    [Fact]
    public async Task Reopen_Draft_Throws()
    {
        await using var context = new InMemoryDatabaseContext();
        var user = await new UserFake(Role.Interviewer).PleaseAsync(context);

        var survey = new PublicSurvey("Test", "desc", "slug", user.Id);
        var question = new PublicSurveyQuestion("Q?", 0, survey);
        survey.Questions.Add(question);
        question.AddOption("A", 0);
        question.AddOption("B", 1);
        context.PublicSurveys.Add(survey);
        context.SaveChanges();

        context.ChangeTracker.Clear();

        await Assert.ThrowsAsync<BadRequestException>(() =>
            new ReopenPublicSurveyHandler(context, new FakeAuth(user))
                .Handle(survey.Id, default));
    }

    private static PublicSurvey CreateClosedSurvey(
        Domain.Entities.Users.User user,
        InMemoryDatabaseContext context)
    {
        var survey = new PublicSurvey("Test", "desc", "slug", user.Id);
        var question = new PublicSurveyQuestion("Q?", 0, survey);
        survey.Questions.Add(question);
        question.AddOption("A", 0);
        question.AddOption("B", 1);
        context.PublicSurveys.Add(survey);
        context.SaveChanges();

        survey.Publish();
        context.SaveChanges();

        survey.Close();
        context.SaveChanges();

        return survey;
    }
}
