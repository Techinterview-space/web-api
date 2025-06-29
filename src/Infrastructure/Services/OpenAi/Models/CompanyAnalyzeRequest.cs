using Domain.Entities.Companies;

namespace Infrastructure.Services.OpenAi.Models;

internal record CompanyAnalyzeRequest
{
    public CompanyAnalyzeRequest(
        Company company)
    {
        Name = company.Name;
        Rating = company.Rating;
        RatingHistoryRecords = company.RatingHistory?
            .Select(history => new CompanyRatingHistoryItem(history))
            .ToList();

        Reviews = company.Reviews
            .Select(x => new CompanyReviewData(x))
            .ToList();
    }

    public double Rating { get; }

    public string Name { get; }

    public List<CompanyRatingHistoryItem> RatingHistoryRecords { get; }

    public List<CompanyReviewData> Reviews { get; }
}

internal record CompanyReviewData
{
    public CompanyReviewData(
        CompanyReview review)
    {
        Pros = review.Pros;
        Cons = review.Cons;
        CultureAndValues = review.CultureAndValues;
        CodeQuality = review.CodeQuality;
        WorkLifeBalance = review.WorkLifeBalance;
        Management = review.Management;
        CompensationAndBenefits = review.CompensationAndBenefits;
        CareerOpportunities = review.CareerOpportunities;
        TotalRating = review.TotalRating;
        LikesCount = review.LikesCount;
        DislikesCount = review.DislikesCount;
        TotalRating = review.TotalRating;
    }

    public string Pros { get; }

    public string Cons { get; }

    public int CultureAndValues { get; }

    public int? CodeQuality { get; }

    public int WorkLifeBalance { get; }

    public int Management { get; }

    public int CompensationAndBenefits { get; }

    public int CareerOpportunities { get; }

    public double TotalRating { get; }

    public int LikesCount { get; }

    public int DislikesCount { get; }
}

internal record CompanyRatingHistoryItem
{
    public CompanyRatingHistoryItem(
        CompanyRatingHistory history)
    {
        CreatedAt = history.CreatedAt;
        Rating = history.Rating;
    }

    public DateTime CreatedAt { get; }

    public double Rating { get; }
}