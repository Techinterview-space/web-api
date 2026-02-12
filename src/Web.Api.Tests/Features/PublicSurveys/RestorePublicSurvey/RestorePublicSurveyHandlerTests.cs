using System.Linq;
using System.Threading.Tasks;
using Domain.Entities.Surveys;
using Domain.Enums;
using Domain.Validation.Exceptions;
using TestUtils.Auth;
using TestUtils.Db;
using TestUtils.Fakes;
using Web.Api.Features.PublicSurveys.RestorePublicSurvey;
using Xunit;

namespace Web.Api.Tests.Features.PublicSurveys.RestorePublicSurvey;

public class RestorePublicSurveyHandlerTests
{
    [Fact]
    public async Task Restore_DeletedSurvey_Ok()
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

        survey.Delete();
        context.SaveChanges();

        context.ChangeTracker.Clear();

        var result = await new RestorePublicSurveyHandler(context, new FakeAuth(user))
            .Handle(survey.Id, default);

        Assert.Null(result.DeletedAt);
    }

    [Fact]
    public async Task Restore_NotDeleted_Throws()
    {
        await using var context = new InMemoryDatabaseContext();
        var user = await new UserFake(Role.Interviewer).PleaseAsync(context);

        var survey = new PublicSurvey("Test", "desc", "slug", user.Id);
        context.PublicSurveys.Add(survey);
        context.SaveChanges();

        context.ChangeTracker.Clear();

        await Assert.ThrowsAsync<BadRequestException>(() =>
            new RestorePublicSurveyHandler(context, new FakeAuth(user))
                .Handle(survey.Id, default));
    }
}
