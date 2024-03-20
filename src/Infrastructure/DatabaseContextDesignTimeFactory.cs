using Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Infrastructure;

public class DatabaseContextDesignTimeFactory : DatabaseContextDesignTimeFactoryBase<DatabaseContext>
{
    protected override DatabaseContext CreateContext(DbContextOptionsBuilder<DatabaseContext> builder)
    {
        builder.UseNpgsql(ConnectionString);

        return new DatabaseContext(builder.Options);
    }

    protected override string ConnectionString => Configuration.GetConnectionString("Database");

    public DatabaseContextDesignTimeFactory()
        : base(new DirectoryInfo("../Web.Api").FullName)
    {
    }
}