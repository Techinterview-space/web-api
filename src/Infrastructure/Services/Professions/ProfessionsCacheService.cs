using Domain.Entities.Salaries;
using Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace Infrastructure.Services.Professions;

public class ProfessionsCacheService : IProfessionsCacheService
{
    private const string CacheKey = nameof(ProfessionsCacheService);

    private readonly DatabaseContext _context;
    private readonly IMemoryCache _cache;

    public ProfessionsCacheService(
        IMemoryCache cache,
        DatabaseContext context)
    {
        _cache = cache;
        _context = context;
    }

    public async Task<List<Profession>> GetProfessionsAsync(
        CancellationToken cancellationToken = default)
    {
        return await _cache.GetOrCreateAsync(
            CacheKey + "__AllProfessions",
            async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(120);
                return await _context
                    .Professions
                    .AsNoTracking()
                    .ToListAsync(cancellationToken);
            });
    }
}