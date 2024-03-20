using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Infrastructure.Database;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using TechInterviewer.Features.Labels.Models;
using TechInterviewer.Features.Salaries.Models;

namespace TechInterviewer.Features.Salaries.GetSelectBoxItems;

public class GetSelectBoxItemsHandler : IRequestHandler<GetSelectBoxItemsQuery, SelectBoxItemsResponse>
{
    private const string CacheKey = $"{nameof(GetSelectBoxItemsHandler)}_Response";
    private const int CacheDurationMinutes = 60;

    private readonly DatabaseContext _context;
    private readonly IMemoryCache _cache;

    public GetSelectBoxItemsHandler(
        DatabaseContext context,
        IMemoryCache cache)
    {
        _context = context;
        _cache = cache;
    }

    public async Task<SelectBoxItemsResponse> Handle(
        GetSelectBoxItemsQuery request,
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