using System.Linq;
using System.Threading.Tasks;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;
using TestUtils.Db;
using TestUtils.Fakes;
using Xunit;

namespace Domain.Tests.Entities.Companies;

public class CompanyTest
{
    [Fact]
    public async Task ApproveReview_CoupleOfReviewsApproved_StatsRecalculatedEachTime()
    {
        await using var context = new InMemoryDatabaseContext();
        var company = new CompanyFake().Please(context);
        var user = await new UserFake(Role.Interviewer).PleaseAsync(context);

        var review1 = new CompanyReviewFake(company, user).Please(context);

        company = context.Companies
            .Include(c => c.Reviews)
            .Include(c => c.RatingHistory)
            .First(c => c.Id == company.Id);

        Assert.Empty(company.RatingHistory);
        Assert.Single(company.Reviews);
        Assert.Equal(0, company.Rating);

        // Act #1
        company.ApproveReview(review1.Id);
        await context.SaveChangesAsync();

        company = context.Companies
            .Include(c => c.Reviews)
            .Include(c => c.RatingHistory)
            .First(c => c.Id == company.Id);

        Assert.Single(company.RatingHistory);
        Assert.NotEqual(0, company.Rating);
        Assert.Equal(company.Rating, company.RatingHistory[0].Rating);

        var previousRating = company.Rating;

        var review2 = new CompanyReviewFake(company, user).Please(context);
        company = context.Companies
            .Include(c => c.Reviews)
            .Include(c => c.RatingHistory)
            .First(c => c.Id == company.Id);

        Assert.Equal(2, company.Reviews.Count);

        // Act #2
        company.ApproveReview(review2.Id);
        await context.SaveChangesAsync();

        company = context.Companies
            .Include(c => c.Reviews)
            .Include(c => c.RatingHistory)
            .First(c => c.Id == company.Id);

        Assert.Equal(2, company.RatingHistory.Count);
        Assert.NotEqual(0, company.Rating);
        Assert.NotEqual(previousRating, company.Rating);
        Assert.Equal(company.Rating, company.RatingHistory[1].Rating);
    }

    [Fact]
    public async Task MarkAsOutdated_CoupleOfReviews_StatsRecalculatedEachTime()
    {
        await using var context = new InMemoryDatabaseContext();
        var company = new CompanyFake().Please(context);
        var user = await new UserFake(Role.Interviewer).PleaseAsync(context);

        var review1 = new CompanyReviewFake(company, user).Please(context);
        var review2 = new CompanyReviewFake(company, user).Please(context);
        var review3 = new CompanyReviewFake(company, user).Please(context);

        company = context.Companies
            .Include(c => c.Reviews)
            .Include(c => c.RatingHistory)
            .First(c => c.Id == company.Id);

        company.ApproveReview(review1.Id);
        company.ApproveReview(review2.Id);
        company.ApproveReview(review3.Id);
        await context.SaveChangesAsync();

        company = context.Companies
            .Include(c => c.Reviews)
            .Include(c => c.RatingHistory)
            .First(c => c.Id == company.Id);

        Assert.Equal(3, company.GetRelevantReviews().Count);
        Assert.Equal(3, company.RatingHistory.Count);
        Assert.NotEqual(0, company.Rating);

        var previousRating = company.Rating;

        company.MarkReviewAsOutdated(review1.Id);
        await context.SaveChangesAsync();

        company = context.Companies
            .Include(c => c.Reviews)
            .Include(c => c.RatingHistory)
            .First(c => c.Id == company.Id);

        Assert.Equal(4, company.RatingHistory.Count);
        Assert.NotEqual(0, company.Rating);
        Assert.NotEqual(previousRating, company.Rating);
        Assert.Equal(2, company.GetRelevantReviews().Count);
    }
}