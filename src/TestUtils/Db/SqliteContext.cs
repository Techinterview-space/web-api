using System.Data.Common;
using System.Threading.Tasks;
using Infrastructure.Database;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace TestUtils.Db;

// https://docs.microsoft.com/en-us/ef/core/testing/sqlite#using-sqlite-in-memory-databases
public class SqliteContext : DatabaseContext
{
    private readonly DbConnection _connection;

    public SqliteContext()
        : base(new DbContextOptionsBuilder<DatabaseContext>()
            .UseSqlite(new SqliteConnection("Filename=:memory:"))
            .EnableSensitiveDataLogging()
            .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options)
    {
        _connection = Database.GetDbConnection();
        _connection.Open();

        Database.EnsureDeleted();
        Database.EnsureCreated();
    }

    public override void Dispose()
    {
        Database.EnsureDeleted();
        _connection?.Dispose();
        base.Dispose();
    }

    public override async ValueTask DisposeAsync()
    {
        await Database.EnsureDeletedAsync();
        await _connection.DisposeAsync();
        await base.DisposeAsync();
    }
}