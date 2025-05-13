using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Web.Api.Features.BackgroundJobs;

public class CompanySlugGenerateJob : InvocableJobBase<CompanySlugGenerateJob>
{
    private readonly DatabaseContext _context;

    public CompanySlugGenerateJob(
        ILogger<CompanySlugGenerateJob> logger,
        DatabaseContext context)
        : base(logger)
    {
        _context = context;
    }

    public override async Task ExecuteAsync(
        CancellationToken cancellationToken = default)
    {
        var companies = await _context.Companies
            .Where(c => c.Slug == null)
            .ToListAsync(cancellationToken);

        if (companies.Count == 0)
        {
            Logger.LogInformation("No companies to be updated");
            return;
        }

        foreach (var company in companies)
        {
            company.GenerateSlug();
        }

        await _context.TrySaveChangesAsync(cancellationToken);
        Logger.LogInformation("Company slug generated. {Count} companies updated", companies.Count);
    }
}