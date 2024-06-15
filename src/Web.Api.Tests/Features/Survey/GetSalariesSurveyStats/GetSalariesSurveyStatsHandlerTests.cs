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
        var user1 = await new FakeUser(Role.Interviewer).PleaseAsync(context);
        var user2 = await new FakeUser(Role.Interviewer).PleaseAsync(context);

        var reply1 = await new FakeSalariesSurveyReply(
            SurveyUsefulnessReplyType.Yes,
            ExpectationReplyType.Expected,
            user1)
            .PleaseAsync(context);

        var reply2 = await new FakeSalariesSurveyReply(
                SurveyUsefulnessReplyType.Yes,
                ExpectationReplyType.Expected,
                user2)
            .PleaseAsync(context);

        var reply3 = await new FakeSalariesSurveyReply(
                SurveyUsefulnessReplyType.No,
                ExpectationReplyType.MoreThanExpected,
                user1)
            .PleaseAsync(context);

        var reply4 = await new FakeSalariesSurveyReply(
                SurveyUsefulnessReplyType.No,
                ExpectationReplyType.MoreThanExpected,
                user2)
            .PleaseAsync(context);

        var reply5 = await new FakeSalariesSurveyReply(
                SurveyUsefulnessReplyType.Yes,
                ExpectationReplyType.MoreThanExpected,
                user1)
            .PleaseAsync(context);

        var handler = new GetSalariesSurveyStatsHandler(context);
        var result = await handler.Handle(new GetSalariesSurveyStatsQuery(), default);

        Assert.Equal(5, result.CountOfRecords);

        Assert.Equal(
            3,
            result.UsefulnessData.First(x => x.ReplyType == SurveyUsefulnessReplyType.Yes).Data.CountOfReplies);

        Assert.Equal(
            2,
            result.UsefulnessData.First(x => x.ReplyType == SurveyUsefulnessReplyType.No).Data.CountOfReplies);

        Assert.Equal(
            0,
            result.UsefulnessData.First(x => x.ReplyType == SurveyUsefulnessReplyType.NotSure).Data.CountOfReplies);

        Assert.Equal(
            2,
            result.ExpectationData.First(x => x.ReplyType == ExpectationReplyType.Expected).Data.CountOfReplies);

        Assert.Equal(
            3,
            result.ExpectationData.First(x => x.ReplyType == ExpectationReplyType.MoreThanExpected).Data.CountOfReplies);

        Assert.Equal(
            0,
            result.ExpectationData.First(x => x.ReplyType == ExpectationReplyType.LessThanExpected).Data.CountOfReplies);
    }
}