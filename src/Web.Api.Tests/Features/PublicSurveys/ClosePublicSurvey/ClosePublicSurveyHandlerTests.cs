using System.Threading.Tasks;
using Domain.Entities.Surveys;
using Domain.Enums;
using Domain.Validation.Exceptions;
using TestUtils.Auth;
using TestUtils.Db;
using TestUtils.Fakes;
using Web.Api.Features.PublicSurveys.ClosePublicSurvey;
using Xunit;

namespace Web.Api.Tests.Features.PublicSurveys.ClosePublicSurvey;

public class ClosePublicSurveyHandlerTests
{
    [Fact]
    public async Task Close_Published_Ok()
    {
        await using var context = new InMemoryDatabaseContext();
        var user = await new UserFake(Role.Interviewer).PleaseAsync(context);
        var survey = CreatePublishedSurvey(user, context);

        context.ChangeTracker.Clear();

        var result = await new ClosePublicSurveyHandler(context, new FakeAuth(user))
            .Handle(survey.Id, default);

        Assert.Equal(PublicSurveyStatus.Closed, result.Status);
    }

    [Fact]
    public async Task Close_Draft_Throws()
    {
        await using var context = new InMemoryDatabaseContext();
        var user = await new UserFake(Role.Interviewer).PleaseAsync(context);
        var survey = CreateDraftSurvey(user, context);

        context.ChangeTracker.Clear();

        await Assert.ThrowsAsync<BadRequestException>(() =>
            new ClosePublicSurveyHandler(context, new FakeAuth(user))
                .Handle(survey.Id, default));
    }

    private static PublicSurvey CreateDraftSurvey(
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
