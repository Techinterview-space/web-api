using System;
using System.Collections.Generic;
using Domain.Entities.Companies;
using Infrastructure.Services.Companies;

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

    public int ViewsCount { get; init; }

    public string Slug { get; init; }

    public List<CompanyReviewDto> Reviews { get; init; }

    public List<CompanyRatingHistoryRecordDto> RatingHistory { get; init; }

    public AiHtmlAnalysis AiAnalysis { get; init; }

    public DateTimeOffset CreatedAt { get; init; }

    public DateTimeOffset UpdatedAt { get; init; }

    public DateTime? DeletedAt { get; init; }

    public CompanyDto()
    {
    }

    public CompanyDto(
        Company company)
    {
        Id = company.Id;
        Name = company.Name;
        Description = company.Description;
        Links = company.Links;
        LogoUrl = company.LogoUrl;
        Rating = company.Rating;
        ReviewsCount = company.ReviewsCount;
        ViewsCount = company.ViewsCount;
        Slug = company.Slug;
        Reviews = company.Reviews?.ConvertAll(review => new CompanyReviewDto(review));
        RatingHistory = company.RatingHistory?.ConvertAll(record => new CompanyRatingHistoryRecordDto(record));
        if (company.HasAiAnalysis())
        {
            AiAnalysis = new AiHtmlAnalysis(company);
        }

        CreatedAt = company.CreatedAt;
        UpdatedAt = company.UpdatedAt;
        DeletedAt = company.DeletedAt;
    }
}