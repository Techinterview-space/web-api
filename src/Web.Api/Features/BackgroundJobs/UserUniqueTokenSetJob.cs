using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Web.Api.Features.BackgroundJobs;

public class UserUniqueTokenSetJob
    : InvocableJobBase<UserUniqueTokenSetJob>
{
    private readonly DatabaseContext _context;

    public UserUniqueTokenSetJob(
        ILogger<UserUniqueTokenSetJob> logger,
        DatabaseContext context)
        : base(logger)
    {
        _context = context;
    }

    public override async Task ExecuteAsync(
        CancellationToken cancellationToken = default)
    {
        var users = await _context.Users
            .Where(x => x.UniqueToken == null)
            .OrderByDescending(x => x.CreatedAt)
            .Take(100)
            .ToListAsync(cancellationToken);

        if (users.Count == 0)
        {
            Logger.LogInformation(
                "No users found without a unique token. Skipping");

            return;
        }

        foreach (var user in users)
        {
            user.GenerateNewEmailUnsubscribeTokenIfNecessary();
        }

        await _context.SaveChangesAsync(cancellationToken);
        Logger.LogInformation(
            "Set unique tokens for {Count} users",
            users.Count);
    }
}