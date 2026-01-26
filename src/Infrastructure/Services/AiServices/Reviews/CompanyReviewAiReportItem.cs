using System.Linq.Expressions;
using Domain.Entities.Companies;

namespace Infrastructure.Services.AiServices.Reviews;

public record CompanyReviewAiReportItem
{
    public int CultureAndValues { get; init; }

    public int? CodeQuality { get; init; }

    public int WorkLifeBalance { get; init; }

    public int Management { get; init; }

    public int CompensationAndBenefits { get; init; }

    public int CareerOpportunities { get; init; }

    public string Pros { get; init; }

    public string Cons { get; init; }

    public string CompanySlug { get; init; }

    public string CompanyName { get; init; }

    public double TotalReviewsCount { get; init; }

    public double TotalCompanyRating { get; init; }

    public DateTimeOffset CreatedAt { get; init; }

    public int LikesCount { get; init; }

    public int DislikesCount { get; init; }

    public double? LikesRate
    {
        get
        {
            if (LikesCount == 0 && DislikesCount == 0)
            {
                return null;
            }

            return Math.Round(
                (double)LikesCount / (LikesCount + DislikesCount) * 100);
        }
    }

    public CompanyReviewAiReportItem(
        CompanyReview review)
    {
        CultureAndValues = review.CultureAndValues;
        CodeQuality = review.CodeQuality;
        WorkLifeBalance = review.WorkLifeBalance;
        Management = review.Management;
        CompensationAndBenefits = review.CompensationAndBenefits;
        CareerOpportunities = review.CareerOpportunities;
        Pros = review.Pros;
        Cons = review.Cons;
        LikesCount = review.LikesCount;
        DislikesCount = review.DislikesCount;
        CreatedAt = review.CreatedAt;

        CompanyName = review.Company?.Name;
        CompanySlug = review.Company?.Slug;
        TotalReviewsCount = review.Company?.ReviewsCount ?? 0;
        TotalCompanyRating = review.Company?.Rating ?? 0;
    }

    public CompanyReviewAiReportItem()
    {
    }

    public static Expression<Func<CompanyReview, CompanyReviewAiReportItem>> Transformation =>
        x => new CompanyReviewAiReportItem
        {
            CultureAndValues = x.CultureAndValues,
            CodeQuality = x.CodeQuality,
            WorkLifeBalance = x.WorkLifeBalance,
            Management = x.Management,
            CompensationAndBenefits = x.CompensationAndBenefits,
            CareerOpportunities = x.CareerOpportunities,
            Pros = x.Pros,
            Cons = x.Cons,
            LikesCount = x.LikesCount,
            DislikesCount = x.DislikesCount,
            CreatedAt = x.CreatedAt,
            CompanyName = x.Company != null
                ? x.Company.Name
                : null,
            CompanySlug = x.Company != null
                ? x.Company.Slug
                : null,
            TotalReviewsCount = x.Company != null
                ? x.Company.ReviewsCount
                : 0,
            TotalCompanyRating = x.Company != null
                ? x.Company.Rating
                : 0
        };
}