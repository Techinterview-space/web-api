using System.Linq;
using System.Threading.Tasks;
using Domain.Entities.Surveys;
using Domain.Enums;
using Domain.Validation.Exceptions;
using TestUtils.Auth;
using TestUtils.Db;
using TestUtils.Fakes;
using Web.Api.Features.PublicSurveys.DeletePublicSurvey;
using Xunit;

namespace Web.Api.Tests.Features.PublicSurveys.DeletePublicSurvey;

public class DeletePublicSurveyHandlerTests
{
    [Fact]
    public async Task Delete_ExistingSurvey_Ok()
    {
        await using var context = new InMemoryDatabaseContext();
        var user = await new UserFake(Role.Interviewer).PleaseAsync(context);

        var survey = new PublicSurvey("Test", "desc", "slug", user.Id);
        context.PublicSurveys.Add(survey);
        await context.SaveChangesAsync();

        context.ChangeTracker.Clear();

        await new DeletePublicSurveyHandler(context, new FakeAuth(user))
            .Handle(survey.Id, default);

        var deleted = context.PublicSurveys.First(s => s.Id == survey.Id);
        Assert.NotNull(deleted.DeletedAt);
    }

    [Fact]
    public async Task Delete_NotAuthor_Throws()
    {
        await using var context = new InMemoryDatabaseContext();
        var author = await new UserFake(Role.Interviewer).PleaseAsync(context);
        var other = await new UserFake(Role.Interviewer).PleaseAsync(context);

        var survey = new PublicSurvey("Test", "desc", "slug", author.Id);
        context.PublicSurveys.Add(survey);
        await context.SaveChangesAsync();

        context.ChangeTracker.Clear();

        await Assert.ThrowsAsync<NotFoundException>(() =>
            new DeletePublicSurveyHandler(context, new FakeAuth(other))
                .Handle(survey.Id, default));
    }
}
