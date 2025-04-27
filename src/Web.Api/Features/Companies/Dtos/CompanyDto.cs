using System;
using System.Collections.Generic;
using Domain.Entities.Companies;

namespace Web.Api.Features.Companies.Dtos;

public record CompanyDto
{
    public Guid Id { get; init; }

    public string Name { get; init; }

    public string Description { get; init; }

    public List<string> Links { get; init; }

    public string LogoUrl { get; init; }

    public double Rating { get; init; }

    public int ReviewsCount { get; init; }

    public List<CompanyReviewDto> Reviews { get; init; }

    public List<CompanyRatingHistoryRecordDto> RatingHistory { get; init; }

    public DateTime? DeletedAt { get; init; }

    public bool UserIsAllowedToLeaveReview { get; init; }

    public CompanyDto()
    {
    }

    public CompanyDto(
        Company company,
        bool userIsAllowedToLeaveReview = false)
    {
        Id = company.Id;
        Name = company.Name;
        Description = company.Description;
        Links = company.Links;
        LogoUrl = company.LogoUrl;
        Rating = company.Rating;
        ReviewsCount = company.ReviewsCount;
        Reviews = company.Reviews?.ConvertAll(review => new CompanyReviewDto(review));
        RatingHistory = company.RatingHistory?.ConvertAll(record => new CompanyRatingHistoryRecordDto(record));
        DeletedAt = company.DeletedAt;
        UserIsAllowedToLeaveReview = userIsAllowedToLeaveReview;
    }
}