using System;
using System.Threading;
using System.Threading.Tasks;
using Domain.Validation;
using Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace TestUtils.Db;

public class InMemoryDatabaseContext : DatabaseContext
{
    public InMemoryDatabaseContext()
        : base(new DbContextOptionsBuilder<DatabaseContext>()
            .EnableSensitiveDataLogging()

            // https://github.com/dotnet/efcore/issues/12459#issuecomment-399994558
            // .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking)
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .ConfigureWarnings(x =>
            {
                x.Ignore(InMemoryEventId.TransactionIgnoredWarning);
                x.Log(RelationalEventId.MultipleCollectionIncludeWarning);
            })
            .Options)
    {
        Database.EnsureDeleted();
        Database.EnsureCreated();
    }

    public override async Task<TEntity> SaveAsync<TEntity>(
        TEntity entity,
        CancellationToken cancellationToken = default)
        where TEntity : class
    {
        var entry = await AddAsync(entity, cancellationToken);
        await TrySaveChangesAsync(cancellationToken);

        return entry.Entity;
    }

    protected override bool IsInMemory()
    {
        return true;
    }
}