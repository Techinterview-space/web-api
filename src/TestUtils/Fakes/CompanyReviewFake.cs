using System;
using System.Collections.Generic;
using Domain.Entities.Companies;
using Domain.Entities.Users;
using Faker;
using TestUtils.Db;
using Company = Domain.Entities.Companies.Company;

namespace TestUtils.Fakes;

public class CompanyReviewFake : CompanyReview
{
    public CompanyReviewFake(
        Company company,
        User user)
        : base(
            RandomNumber.Next(1, 5),
            RandomNumber.Next(1, 5),
            RandomNumber.Next(1, 5),
            RandomNumber.Next(1, 5),
            RandomNumber.Next(1, 5),
            RandomNumber.Next(1, 5),
            Lorem.Sentence(),
            Lorem.Sentence(),
            true,
            CompanyEmploymentType.FullTime,
            company,
            user)
    {
    }

    public CompanyReviewFake SetOutdatedAt(
        DateTime date)
    {
        OutdatedAt = date;
        return this;
    }

    public CompanyReviewFake SetApprovedAt(
        DateTime date)
    {
        ApprovedAt = date;
        OutdatedAt = null;
        return this;
    }

    public CompanyReviewFake SetCreatedAt(
        DateTimeOffset date)
    {
        CreatedAt = date;
        return this;
    }

    public CompanyReviewFake AddVoteByFake(
        User user,
        CompanyReviewVoteType voteType)
    {
        Votes ??= new List<CompanyReviewVote>();
        AddVote(
            user,
            voteType);

        return this;
    }

    public CompanyReview Please(
        InMemoryDatabaseContext context)
    {
        var entry = context.CompanyReviews.Add((CompanyReview)this);
        context.SaveChanges();

        return entry.Entity;
    }
}