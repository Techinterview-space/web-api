using System;
using Domain.Entities.Companies;

namespace Web.Api.Features.Companies.Dtos;

public record CompanyRatingHistoryRecordDto
{
    public CompanyRatingHistoryRecordDto()
    {
    }

    public CompanyRatingHistoryRecordDto(
        CompanyRatingHistory record)
    {
        CreatedAt = record.CreatedAt;
        Rating = record.Rating;
    }

    public DateTime CreatedAt { get; init; }

    public double Rating { get; init; }
}