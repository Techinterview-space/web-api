using System.Data.Common;
using MG.Utils.Abstract;
using MG.Utils.AspNetCore.HealthCheck;
using Microsoft.Extensions.Configuration;
using Npgsql;

namespace TechInterviewer.Setup.Healthcheck;

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