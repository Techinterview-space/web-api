using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Infrastructure.Database;
using Infrastructure.Services.Global;
using Infrastructure.Services.Mediator;
using Microsoft.EntityFrameworkCore;

namespace Web.Api.Features.Sitemaps;

public class GetSitemapHandler
    : IRequestHandler<Nothing, SitemapUrlSet>
{
    private readonly DatabaseContext _context;
    private readonly IGlobal _global;

    public GetSitemapHandler(
        DatabaseContext context,
        IGlobal global)
    {
        _context = context;
        _global = global;
    }

    public async Task<SitemapUrlSet> Handle(
        Nothing request,
        CancellationToken cancellationToken)
    {
        var urls = new List<SitemapUrl>();

        AddStaticPages(urls);
        await AddCompanyPagesAsync(urls, cancellationToken);

        return new SitemapUrlSet { Urls = urls };
    }

    private void AddStaticPages(
        List<SitemapUrl> urls)
    {
        var baseUrl = _global.FrontendBaseUrl;
        urls.Add(new SitemapUrl
        {
            Loc = baseUrl + "/",
            ChangeFreq = "daily",
            Priority = "1.0",
        });

        urls.Add(new SitemapUrl
        {
            Loc = baseUrl + "/companies",
            ChangeFreq = "daily",
            Priority = "1.0",
        });

        urls.Add(new SitemapUrl
        {
            Loc = baseUrl + "/companies/recent-reviews",
            ChangeFreq = "daily",
            Priority = "0.9",
        });

        urls.Add(new SitemapUrl
        {
            Loc = baseUrl + "/salaries/overview",
            ChangeFreq = "weekly",
            Priority = "1.0",
        });

        urls.Add(new SitemapUrl
        {
            Loc = baseUrl + "/about-us",
            ChangeFreq = "monthly",
            Priority = "0.5",
        });

        urls.Add(new SitemapUrl
        {
            Loc = baseUrl + "/about-telegram-bot",
            ChangeFreq = "monthly",
            Priority = "0.5",
        });

        urls.Add(new SitemapUrl
        {
            Loc = baseUrl + "/agreements/privacy-policy",
            ChangeFreq = "monthly",
            Priority = "0.3",
        });
    }

    private async Task AddCompanyPagesAsync(
        List<SitemapUrl> urls,
        CancellationToken cancellationToken)
    {
        var baseUrl = _global.FrontendBaseUrl;
        var companies = await _context.Companies
            .AsNoTracking()
            .Where(x => x.DeletedAt == null)
            .OrderByDescending(x => x.ReviewsCount)
            .ThenByDescending(x => x.ViewsCount)
            .Select(x => new
            {
                x.Slug,
                x.UpdatedAt,
                x.ReviewsCount,
            })
            .ToListAsync(cancellationToken);

        foreach (var company in companies)
        {
            urls.Add(new SitemapUrl
            {
                Loc = $"{baseUrl}/companies/{company.Slug}",
                LastMod = company.UpdatedAt.ToString("yyyy-MM-dd"),
                ChangeFreq = company.ReviewsCount > 0 ? "weekly" : "monthly",
                Priority = company.ReviewsCount > 0 ? "0.8" : "0.6",
            });
        }
    }
}
