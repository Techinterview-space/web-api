using System.IO;
using Domain.Database;
using MG.Utils.Abstract.NonNullableObjects;
using MG.Utils.EFCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Domain;

public class DatabaseContextDesignTimeFactory : DatabaseContextDesignTimeFactoryBase<DatabaseContext>
{
    protected override DatabaseContext CreateContext(DbContextOptionsBuilder<DatabaseContext> builder)
    {
        builder.UseNpgsql(ConnectionString);

        return new DatabaseContext(builder.Options);
    }

    protected override NonNullableString ConnectionString => new (Configuration.GetConnectionString("Database"));

    public DatabaseContextDesignTimeFactory()
        : base(new NonNullableString(new DirectoryInfo("../Web.Api").FullName))
    {
    }
}