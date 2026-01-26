using System;
using Domain.Entities.Companies;
using Domain.Enums;
using TestUtils.Fakes;
using Xunit;

namespace Domain.Tests.Entities.Companies;

public class CompanyReviewTests
{
    [Fact]
    public void IsRelevant_OutdatedIsNull_True()
    {
        var company = new CompanyFake();
        var user = new UserFake(Role.Interviewer);

        var review = new CompanyReviewFake(
            company,
            user)
            .SetApprovedAt(DateTime.UtcNow);

        Assert.NotNull(review.ApprovedAt);
        Assert.Null(review.OutdatedAt);
        Assert.True(review.IsRelevant());
    }

    [Fact]
    public void IsRelevant_OutdatedIsTooOld_False()
    {
        var company = new CompanyFake();
        var user = new UserFake(Role.Interviewer);

        var review = new CompanyReviewFake(
                company,
                user)
            .SetOutdatedAt(DateTime.UtcNow.AddYears(-CompanyReview.YearsBeforeOutdated).AddDays(-1))
            .SetApprovedAt(DateTime.UtcNow);

        Assert.NotNull(review.ApprovedAt);
        Assert.NotNull(review.OutdatedAt);
        Assert.False(review.IsRelevant());
    }

    [Fact]
    public void IsRelevant_OutdatedIsNotTooOld_False()
    {
        var company = new CompanyFake();
        var user = new UserFake(Role.Interviewer);

        var review = new CompanyReviewFake(
                company,
                user)
            .SetOutdatedAt(DateTime.UtcNow.AddYears(-CompanyReview.YearsBeforeOutdated).AddDays(1))
            .SetApprovedAt(DateTime.UtcNow);

        Assert.NotNull(review.ApprovedAt);
        Assert.NotNull(review.OutdatedAt);
        Assert.False(review.IsRelevant());
    }
}