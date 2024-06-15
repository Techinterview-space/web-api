using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Web.Api.Features.Labels.Models;
using Web.Api.Features.Salaries.Models;

namespace Web.Api.Features.Salaries.Providers;

public class SalaryLabelsProvider : ISalaryLabelsProvider
{
    private const string CacheKey = $"{nameof(SalaryLabelsProvider)}_Response";
    private const int CacheDurationMinutes = 60;

    private readonly DatabaseContext _context;
    private readonly IMemoryCache _cache;

    public SalaryLabelsProvider(
        DatabaseContext context,
        IMemoryCache cache)
    {
        _context = context;
        _cache = cache;
    }

    public async Task ResetCacheAsync(
        CancellationToken cancellationToken)
    {
        _cache.Remove(CacheKey);
        await GetAsync(cancellationToken);
    }

    public async Task<SelectBoxItemsResponse> GetAsync(
        CancellationToken cancellationToken)
    {
        return await _cache.GetOrCreateAsync(
            CacheKey,
            async (cacheEntry) =>
            {
                cacheEntry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(CacheDurationMinutes);

                return new SelectBoxItemsResponse
                {
                    Skills = await _context.Skills
                        .Select(x => new LabelEntityDto
                        {
                            Id = x.Id,
                            Title = x.Title,
                            HexColor = x.HexColor,
                        })
                        .AsNoTracking()
                        .ToListAsync(cancellationToken),
                    Industries = await _context.WorkIndustries
                        .Select(x => new LabelEntityDto
                        {
                            Id = x.Id,
                            Title = x.Title,
                            HexColor = x.HexColor,
                        })
                        .AsNoTracking()
                        .ToListAsync(cancellationToken),
                    Professions = await _context.Professions
                        .Select(x => new LabelEntityDto
                        {
                            Id = x.Id,
                            Title = x.Title,
                            HexColor = x.HexColor,
                        })
                        .AsNoTracking()
                        .ToListAsync(cancellationToken),
                };
            });
    }
}