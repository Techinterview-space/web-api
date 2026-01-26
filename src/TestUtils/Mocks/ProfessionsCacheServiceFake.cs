using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Domain.Entities.Salaries;
using Infrastructure.Database;
using Infrastructure.Services.Professions;
using Microsoft.EntityFrameworkCore;

namespace TestUtils.Mocks;

public class ProfessionsCacheServiceFake : IProfessionsCacheService
{
    private readonly DatabaseContext _context;

    public ProfessionsCacheServiceFake(
        DatabaseContext context)
    {
        _context = context;
    }

    public Task<List<Profession>> GetProfessionsAsync(
        CancellationToken cancellationToken = default)
    {
        return _context
            .Professions
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }
}