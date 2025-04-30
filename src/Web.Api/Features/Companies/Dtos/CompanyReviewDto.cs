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

    public string CompanyName { get; init; }

    public DateTimeOffset CreatedAt { get; init; }

    public DateTime? ApprovedAt { get; init; }

    public DateTime? OutdatedAt { get; init; }

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
        CreatedAt = review.CreatedAt;
        ApprovedAt = review.ApprovedAt;
        OutdatedAt = review.OutdatedAt;
    }

    public static Expression<Func<CompanyReview, CompanyReviewDto>> Transformation =>
        company => new CompanyReviewDto
        {
            Id = company.Id,
            CultureAndValues = company.CultureAndValues,
            CodeQuality = company.CodeQuality,
            WorkLifeBalance = company.WorkLifeBalance,
            Management = company.Management,
            CompensationAndBenefits = company.CompensationAndBenefits,
            CareerOpportunities = company.CareerOpportunities,
            TotalRating = company.TotalRating,
            Pros = company.Pros,
            Cons = company.Cons,
            IWorkHere = company.IWorkHere,
            UserEmployment = company.UserEmployment,
            CompanyId = company.CompanyId,
            CompanyName = company.Company != null
                ? company.Company.Name
                : null,
            CreatedAt = company.CreatedAt,
            ApprovedAt = company.ApprovedAt,
            OutdatedAt = company.OutdatedAt
        };
}