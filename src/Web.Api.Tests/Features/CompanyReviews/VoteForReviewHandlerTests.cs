using System.Linq;
using System.Threading.Tasks;
using Domain.Entities.Companies;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;
using TestUtils.Auth;
using TestUtils.Db;
using TestUtils.Fakes;
using Web.Api.Features.CompanyReviews.VoteForReview;
using Xunit;

namespace Web.Api.Tests.Features.CompanyReviews;

public class VoteForReviewHandlerTests
{
    [Fact]
    public async Task Handle_UserHasNoVote_Added()
    {
        await using var context = new InMemoryDatabaseContext();
        var company = new CompanyFake().Please(context);

        var user1 = await new UserFake(Role.Interviewer)
            .PleaseAsync(context);

        var review = new CompanyReviewFake(company, user1).Please(context);

        var user2 = await new UserFake(Role.Interviewer)
            .PleaseAsync(context);

        var target = new VoteForReviewHandler(
            context,
            new FakeAuth(user2));

        context.ChangeTracker.Clear();
        Assert.Equal(0, review.LikesCount);
        Assert.Equal(0, review.DislikesCount);
        Assert.Null(review.GetLikesRateOrNull());

        var result = await target.Handle(
            new VoteForReviewCommand(
                company.Id,
                review.Id,
                CompanyReviewVoteType.Like),
            default);

        Assert.True(result.Result);
        review = context.CompanyReviews
            .Include(x => x.Votes)
            .First(x => x.Id == review.Id);

        Assert.Equal(1, review.LikesCount);
        Assert.Equal(0, review.DislikesCount);
        Assert.Equal(100, review.GetLikesRateOrNull());
    }

    [Fact]
    public async Task Handle_UserHasNoVote_TryingToAddVoteToOwnReview_Added()
    {
        await using var context = new InMemoryDatabaseContext();
        var company = new CompanyFake().Please(context);

        var user1 = await new UserFake(Role.Interviewer)
            .PleaseAsync(context);

        var review = new CompanyReviewFake(company, user1).Please(context);

        var target = new VoteForReviewHandler(
            context,
            new FakeAuth(user1));

        context.ChangeTracker.Clear();
        Assert.Equal(0, review.LikesCount);
        Assert.Equal(0, review.DislikesCount);
        Assert.Null(review.GetLikesRateOrNull());

        var result = await target.Handle(
            new VoteForReviewCommand(
                company.Id,
                review.Id,
                CompanyReviewVoteType.Dislike),
            default);

        Assert.True(result.Result);
        review = context.CompanyReviews
            .Include(x => x.Votes)
            .First(x => x.Id == review.Id);

        Assert.Equal(0, review.LikesCount);
        Assert.Equal(1, review.DislikesCount);
        Assert.Equal(0, review.GetLikesRateOrNull());
    }

    [Theory]
    [InlineData(CompanyReviewVoteType.Like)]
    [InlineData(CompanyReviewVoteType.Dislike)]
    public async Task Handle_UserHasVote_NotAdded(
        CompanyReviewVoteType voteType)
    {
        await using var context = new InMemoryDatabaseContext();
        var company = new CompanyFake().Please(context);

        var user1 = await new UserFake(Role.Interviewer)
            .PleaseAsync(context);

        var user2 = await new UserFake(Role.Interviewer)
            .PleaseAsync(context);

        var review = new CompanyReviewFake(company, user1)
            .AddVoteByFake(user2, CompanyReviewVoteType.Like)
            .Please(context);

        Assert.Single(review.Votes);
        Assert.Equal(1, review.LikesCount);
        Assert.Equal(0, review.DislikesCount);
        Assert.Equal(100, review.GetLikesRateOrNull());

        var target = new VoteForReviewHandler(
            context,
            new FakeAuth(user2));

        context.ChangeTracker.Clear();
        var result = await target.Handle(
            new VoteForReviewCommand(
                company.Id,
                review.Id,
                voteType),
            default);

        Assert.False(result.Result);
        review = context.CompanyReviews
            .Include(x => x.Votes)
            .First(x => x.Id == review.Id);

        Assert.Single(review.Votes);
        Assert.Equal(1, review.LikesCount);
        Assert.Equal(0, review.DislikesCount);
        Assert.Equal(100, review.GetLikesRateOrNull());
    }
}