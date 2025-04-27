using System;
using System.Collections.Generic;
using System.Linq;
using Domain.Validation.Exceptions;

namespace Domain.Entities.Companies;

public class Company : HasDatesBase, IHasIdBase<Guid>
{
    public Guid Id { get; protected set; }

    public string Name { get; protected set; }

    // For searching and indexing
    public string NormalizedName { get; protected set; }

    public string Description { get; protected set; }

    public List<string> Links { get; protected set; }

    public string LogoUrl { get; protected set; }

    public double Rating { get; protected set; }

    public int ReviewsCount { get; protected set; }

    public DateTime? DeletedAt { get; protected set; }

    public virtual List<CompanyRatingHistory> RatingHistory { get; protected set; }

    public virtual List<CompanyReview> Reviews { get; protected set; }

    public Company(
        string name,
        string description,
        List<string> links,
        string logoUrl)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
        NormalizedName = name.ToUpperInvariant();

        Description = description ?? throw new ArgumentNullException(nameof(description));
        Links = links ?? new List<string>();
        LogoUrl = logoUrl;
        Rating = 0;
        ReviewsCount = 0;

        Reviews = new List<CompanyReview>();
        RatingHistory = new List<CompanyRatingHistory>();
    }

    public List<CompanyReview> GetRelevantReviews()
    {
        if (Reviews == null)
        {
            throw new InvalidOperationException("Reviews are not initialized.");
        }

        return Reviews
            .Where(x => x.IsRelevant())
            .ToList();
    }

    public bool IsUserAllowedToLeaveReview(
        long userId)
    {
        return Reviews == null ||
               GetRelevantReviews().All(x => x.UserId != userId);
    }

    public void AddReview(
        CompanyReview review)
    {
        NotDeletedOrFail();

        if (Reviews == null)
        {
            throw new InvalidOperationException("Reviews are not initialized.");
        }

        if (GetRelevantReviews().Any(x => x.UserId == review.UserId))
        {
            throw new BadRequestException("User already has a review.");
        }

        Reviews.Add(review);
    }

    public void ApproveReview(
        Guid reviewId)
    {
        NotDeletedOrFail();

        if (Reviews == null)
        {
            throw new InvalidOperationException("Reviews are not initialized.");
        }

        if (RatingHistory == null)
        {
            throw new InvalidOperationException("Reviews are not initialized.");
        }

        var review = Reviews.FirstOrDefault(x => x.Id == reviewId);
        if (review == null)
        {
            throw new NotFoundException("Review not found");
        }

        review.Approve();
        var relevantReviews = GetRelevantReviews();

        Rating = relevantReviews
            .Select(x => x.TotalRating)
            .DefaultIfEmpty(0)
            .Average();

        ReviewsCount = relevantReviews.Count;
        RatingHistory.Add(new CompanyRatingHistory(Rating, this));
    }

    public void MarkReviewAsOutdated(
        Guid reviewId)
    {
        NotDeletedOrFail();

        if (Reviews == null)
        {
            throw new InvalidOperationException("Reviews are not initialized.");
        }

        var review = Reviews.FirstOrDefault(x => x.Id == reviewId);
        if (review == null)
        {
            throw new NotFoundException("Review not found");
        }

        review.MarkAsOutdated();
        var relevantReviews = GetRelevantReviews();

        Rating = relevantReviews
            .Select(x => x.TotalRating)
            .DefaultIfEmpty(0)
            .Average();

        ReviewsCount = relevantReviews.Count;
        RatingHistory.Add(new CompanyRatingHistory(Rating, this));
    }

    public void Delete()
    {
        if (DeletedAt != null)
        {
            throw new BadRequestException("Can't delete a company.");
        }

        DeletedAt = DateTime.UtcNow;
    }

    public void NotDeletedOrFail()
    {
        if (DeletedAt != null)
        {
            throw new BadRequestException("Cannot mark a review as outdated.");
        }
    }

    protected Company()
    {
    }
}