using System.Data.Common;
using Domain.Validation;
using Microsoft.Extensions.Configuration;
using Npgsql;

namespace Web.Api.Setup.Healthcheck;

public class DatabaseHealthCheck : DatabaseHealthCheckBase
{
    private readonly string _connectionString;

    public DatabaseHealthCheck(IConfiguration configuration)
    {
        configuration.ThrowIfNull(nameof(configuration));

        _connectionString = configuration.GetConnectionString("Database");
    }

    protected override DbConnection Connection()
    {
        return new NpgsqlConnection(_connectionString);
    }
}