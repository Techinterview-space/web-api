using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Infrastructure.Database;
using Infrastructure.Services.Correlation;
using Infrastructure.Services.Mediator;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Web.Api.Features.Users.UnsubscribeUserFromEmails;

public class UnsubscribeUserFromEmailsHandler
    : IRequestHandler<List<string>, bool>
{
    private readonly DatabaseContext _database;
    private readonly ILogger<UnsubscribeUserFromEmailsHandler> _logger;
    private readonly ICorrelationIdAccessor _correlationIdAccessor;

    public UnsubscribeUserFromEmailsHandler(
        DatabaseContext database,
        ILogger<UnsubscribeUserFromEmailsHandler> logger,
        ICorrelationIdAccessor correlationIdAccessor)
    {
        _database = database;
        _logger = logger;
        _correlationIdAccessor = correlationIdAccessor;
    }

    public async Task<bool> Handle(
        List<string> userEmails,
        CancellationToken cancellationToken)
    {
        var userEmailsUpper = userEmails
            .Select(x => x.ToUpperInvariant())
            .ToList();

        var users = await _database.Users
            .Where(x => userEmailsUpper.Contains(x.Email.ToUpper()))
            .ToListAsync(cancellationToken);

        if (users.Count == 0)
        {
            _logger.LogWarning(
                "No users found with emails {Emails}. CorrelationId: {CorrelationId}",
                JsonSerializer.Serialize(userEmails),
                _correlationIdAccessor.GetValue());

            return false;
        }

        foreach (var user in users)
        {
            user.UnsubscribeFromAll();
        }

        await _database.SaveChangesAsync(cancellationToken);

        return true;
    }
}