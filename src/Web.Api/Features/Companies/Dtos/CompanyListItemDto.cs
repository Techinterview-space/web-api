using System;
using System.Collections.Generic;
using Infrastructure.Services.Companies;

namespace Web.Api.Features.Companies.Dtos;

public record CompanyListItemDto
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

    // Left null in the list to avoid loading OpenAI records and rendering Markdown for every company; the full analysis is served by GET /api/companies/{slug}.
    public AiHtmlAnalysis AiAnalysis { get; init; }

    public DateTimeOffset CreatedAt { get; init; }
}
