using System.Linq;
using System.Threading.Tasks;
using Domain.Enums;
using TestUtils.Db;
using TestUtils.Fakes;
using Web.Api.Features.Surveys.Admin.GetSurveyRepliesForAdmin;
using Xunit;

namespace Web.Api.Tests.Features.Survey.Admin;

public class GetSurveyRepliesForAdminHandlerTests
{
    [Fact]
    public async Task Handle_HasReplies_ReturnsPaginatedData()
    {
        await using var context = new InMemoryDatabaseContext();
        var user1 = await new UserFake(Role.Interviewer).PleaseAsync(context);
        var user2 = await new UserFake(Role.Interviewer).PleaseAsync(context);

        await new SalariesSurveyReplyFake(5, user1).PleaseAsync(context);
        await new SalariesSurveyReplyFake(3, user2).PleaseAsync(context);
        await new SalariesSurveyReplyFake(4, user1).PleaseAsync(context);

        var handler = new GetSurveyRepliesForAdminHandler(context);
        var result = await handler.Handle(
            new GetSurveyRepliesForAdminQueryParams(),
            default);

        Assert.Equal(3, result.TotalItems);
        Assert.Equal(3, result.Results.Count);
    }

    [Fact]
    public async Task Handle_HasReplies_ReturnsDtoWithUserEmail()
    {
        await using var context = new InMemoryDatabaseContext();
        var user = await new UserFake(Role.Interviewer).PleaseAsync(context);

        await new SalariesSurveyReplyFake(5, user).PleaseAsync(context);

        var handler = new GetSurveyRepliesForAdminHandler(context);
        var result = await handler.Handle(
            new GetSurveyRepliesForAdminQueryParams(),
            default);

        Assert.Single(result.Results);
        var dto = result.Results.First();
        Assert.Equal(5, dto.UsefulnessRating);
        Assert.Equal(user.Id, dto.CreatedByUserId);
        Assert.Equal(user.Email, dto.CreatedByUserEmail);
    }

    [Fact]
    public async Task Handle_NoReplies_ReturnsEmpty()
    {
        await using var context = new InMemoryDatabaseContext();

        var handler = new GetSurveyRepliesForAdminHandler(context);
        var result = await handler.Handle(
            new GetSurveyRepliesForAdminQueryParams(),
            default);

        Assert.Equal(0, result.TotalItems);
        Assert.Empty(result.Results);
    }

    [Fact]
    public async Task Handle_Pagination_ReturnsCorrectPage()
    {
        await using var context = new InMemoryDatabaseContext();
        var user = await new UserFake(Role.Interviewer).PleaseAsync(context);

        for (var i = 0; i < 5; i++)
        {
            await new SalariesSurveyReplyFake(i + 1, user).PleaseAsync(context);
        }

        var handler = new GetSurveyRepliesForAdminHandler(context);
        var result = await handler.Handle(
            new GetSurveyRepliesForAdminQueryParams
            {
                Page = 1,
                PageSize = 2,
            },
            default);

        Assert.Equal(5, result.TotalItems);
        Assert.Equal(2, result.Results.Count);
    }
}
