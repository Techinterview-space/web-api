using System;
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
        CreatedAt = review.CreatedAt;
        ApprovedAt = review.ApprovedAt;
        OutdatedAt = review.OutdatedAt;
    }
}