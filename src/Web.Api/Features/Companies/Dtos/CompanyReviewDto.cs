using System;
using System.Linq.Expressions;
using Domain.Entities.Companies;

namespace Web.Api.Features.Companies.Dtos;

public record CompanyReviewDto
{
    public Guid Id { get; init; }

    public int CultureAndValues { get; init; }

    public int? CodeQuality { get; init; }

    public int WorkLifeBalance { get; init; }

    public int Management { get; init; }

    public int CompensationAndBenefits { get; init; }

    public int CareerOpportunities { get; init; }

    public double TotalRating { get; init; }

    public string Pros { get; init; }

    public string Cons { get; init; }

    public bool IWorkHere { get; init; }

    public CompanyEmploymentType UserEmployment { get; init; }

    public Guid CompanyId { get; init; }

    public string CompanySlug { get; init; }

    public string CompanyName { get; init; }

    public DateTimeOffset CreatedAt { get; init; }

    public DateTime? ApprovedAt { get; init; }

    public DateTime? OutdatedAt { get; init; }

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

    public CompanyReviewDto()
    {
    }

    public CompanyReviewDto(
        CompanyReview review)
    {
        Id = review.Id;
        CultureAndValues = review.CultureAndValues;
        CodeQuality = review.CodeQuality;
        WorkLifeBalance = review.WorkLifeBalance;
        Management = review.Management;
        CompensationAndBenefits = review.CompensationAndBenefits;
        CareerOpportunities = review.CareerOpportunities;
        TotalRating = review.TotalRating;
        Pros = review.Pros;
        Cons = review.Cons;
        IWorkHere = review.IWorkHere;
        UserEmployment = review.UserEmployment;
        CompanyId = review.CompanyId;
        CompanyName = review.Company?.Name;
        CompanySlug = review.Company?.Slug;
        LikesCount = review.LikesCount;
        DislikesCount = review.DislikesCount;
        CreatedAt = review.CreatedAt;
        ApprovedAt = review.ApprovedAt;
        OutdatedAt = review.OutdatedAt;
    }

    public static Expression<Func<CompanyReview, CompanyReviewDto>> Transformation =>
        x => new CompanyReviewDto
        {
            Id = x.Id,
            CultureAndValues = x.CultureAndValues,
            CodeQuality = x.CodeQuality,
            WorkLifeBalance = x.WorkLifeBalance,
            Management = x.Management,
            CompensationAndBenefits = x.CompensationAndBenefits,
            CareerOpportunities = x.CareerOpportunities,
            TotalRating = x.TotalRating,
            Pros = x.Pros,
            Cons = x.Cons,
            IWorkHere = x.IWorkHere,
            UserEmployment = x.UserEmployment,
            CompanyId = x.CompanyId,
            CompanyName = x.Company != null
                ? x.Company.Name
                : null,
            CompanySlug = x.Company != null
                ? x.Company.Slug
                : null,
            LikesCount = x.LikesCount,
            DislikesCount = x.DislikesCount,
            CreatedAt = x.CreatedAt,
            ApprovedAt = x.ApprovedAt,
            OutdatedAt = x.OutdatedAt
        };
}