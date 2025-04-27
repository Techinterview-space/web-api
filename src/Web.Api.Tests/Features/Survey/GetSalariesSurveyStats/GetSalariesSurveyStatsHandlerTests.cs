using System.Linq;
using System.Threading.Tasks;
using Domain.Entities.Questions;
using Domain.Enums;
using TestUtils.Db;
using TestUtils.Fakes;
using Web.Api.Features.Surveys.GetSalariesSurveyStats;
using Xunit;

namespace Web.Api.Tests.Features.Survey.GetSalariesSurveyStats;

public class GetSalariesSurveyStatsHandlerTests
{
    [Fact]
    public async Task Handle_HasSomeReplies_ReturnsStat()
    {
        await using var context = new InMemoryDatabaseContext();
        var user1 = await new UserFake(Role.Interviewer).PleaseAsync(context);
        var user2 = await new UserFake(Role.Interviewer).PleaseAsync(context);

        var reply1 = await new SalariesSurveyReplyFake(
            5,
            user1)
            .PleaseAsync(context);

        var reply2 = await new SalariesSurveyReplyFake(
                5,
                user2)
            .PleaseAsync(context);

        var reply3 = await new SalariesSurveyReplyFake(
                3,
                user1)
            .PleaseAsync(context);

        var reply4 = await new SalariesSurveyReplyFake(
                4,
                user2)
            .PleaseAsync(context);

        var reply5 = await new SalariesSurveyReplyFake(
                2,
                user1)
            .PleaseAsync(context);

        var handler = new GetSalariesSurveyStatsHandler(context);
        var result = await handler.Handle(new GetSalariesSurveyStatsQuery(), default);

        Assert.Equal(5, result.CountOfRecords);

        Assert.Equal(
            2,
            result.UsefulnessData.First(x => x.RatingValue == 5).Data.CountOfReplies);

        Assert.Equal(
            1,
            result.UsefulnessData.First(x => x.RatingValue == 4).Data.CountOfReplies);

        Assert.Equal(
            1,
            result.UsefulnessData.First(x => x.RatingValue == 3).Data.CountOfReplies);

        Assert.Equal(
            1,
            result.UsefulnessData.First(x => x.RatingValue == 2).Data.CountOfReplies);

        Assert.Equal(
            0,
            result.UsefulnessData.First(x => x.RatingValue == 1).Data.CountOfReplies);
    }
}