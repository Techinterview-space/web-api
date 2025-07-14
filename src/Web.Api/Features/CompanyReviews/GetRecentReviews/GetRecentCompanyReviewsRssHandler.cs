using System;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Web.Api.Features.Companies.Dtos;

namespace Web.Api.Features.CompanyReviews.GetRecentReviews;

public class GetRecentCompanyReviewsRssHandler
    : Infrastructure.Services.Mediator.IRequestHandler<GetRecentCompanyReviewsRssQuery, RssChannel>
{
    private const int MaxPageSize = 100;

    private readonly DatabaseContext _context;

    public GetRecentCompanyReviewsRssHandler(
        DatabaseContext context)
    {
        _context = context;
    }

    public async Task<RssChannel> Handle(
        GetRecentCompanyReviewsRssQuery request,
        CancellationToken cancellationToken)
    {
        var pageSize = request.PageSize > MaxPageSize
            ? MaxPageSize
            : request.PageSize;

        var reviews = await _context.CompanyReviews
            .Include(x => x.Company)
            .Where(x =>
                x.ApprovedAt != null &&
                x.OutdatedAt == null)
            .OrderByDescending(x => x.CreatedAt)
            .Take(pageSize)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        var rssItems = reviews.Select(review => new RssItem
        {
            Title = $"New review for {WebUtility.HtmlEncode(review.Company?.Name ?? "Unknown Company")}",
            Link = $"https://techinterview.space/companies/{review.Company?.Slug}",
            Description = CreateDescription(review),
            PubDate = review.CreatedAt.ToString("R", CultureInfo.InvariantCulture),
            Guid = $"company-review-{review.Id}",
            Category = "Company Reviews"
        }).ToList();

        return new RssChannel
        {
            Channel = new Channel
            {
                Title = "Tech Interview Space - Recent Company Reviews",
                Link = "https://techinterview.space",
                Description = "Latest company reviews from Tech Interview Space",
                LastBuildDate = DateTime.UtcNow.ToString("R", CultureInfo.InvariantCulture),
                Items = rssItems
            }
        };
    }

    private static string CreateDescription(Domain.Entities.Companies.CompanyReview review)
    {
        var rating = review.TotalRating.ToString("F1");
        var employment = review.UserEmployment.ToString();
        
        var description = $"⭐ Overall Rating: {rating}/5.0 &lt;br/&gt;" +
                         $"💼 Employment Type: {WebUtility.HtmlEncode(employment)} &lt;br/&gt;" +
                         $"📊 Detailed Ratings: &lt;br/&gt;" +
                         $"• Culture &amp; Values: {review.CultureAndValues}/5 &lt;br/&gt;" +
                         $"• Work-Life Balance: {review.WorkLifeBalance}/5 &lt;br/&gt;" +
                         $"• Management: {review.Management}/5 &lt;br/&gt;" +
                         $"• Compensation &amp; Benefits: {review.CompensationAndBenefits}/5 &lt;br/&gt;" +
                         $"• Career Opportunities: {review.CareerOpportunities}/5 &lt;br/&gt;";

        if (review.CodeQuality.HasValue)
        {
            description += $"• Code Quality: {review.CodeQuality}/5 &lt;br/&gt;";
        }

        if (!string.IsNullOrWhiteSpace(review.Pros))
        {
            description += $"&lt;br/&gt;✅ Pros: {WebUtility.HtmlEncode(review.Pros)} &lt;br/&gt;";
        }

        if (!string.IsNullOrWhiteSpace(review.Cons))
        {
            description += $"&lt;br/&gt;❌ Cons: {WebUtility.HtmlEncode(review.Cons)}";
        }

        return description;
    }
}