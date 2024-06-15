using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Web.Api.Setup.Healthcheck;

public abstract class DatabaseHealthCheckBase : IHealthCheck
{
    private const string DefaultTestQuery = "Select 1";

    protected abstract DbConnection Connection();

    // Was copied from
    // https://docs.microsoft.com/en-us/dotnet/architecture/microservices/implement-resilient-applications/monitor-app-health
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken)
    {
        await using var connection = Connection();
        try
        {
            await connection.OpenAsync(cancellationToken);

            var command = connection.CreateCommand();
            command.CommandText = DefaultTestQuery;

            await command.ExecuteNonQueryAsync(cancellationToken);
        }
        catch (DbException ex)
        {
            return new HealthCheckResult(status: context.Registration.FailureStatus, exception: ex);
        }

        return HealthCheckResult.Healthy();
    }
}