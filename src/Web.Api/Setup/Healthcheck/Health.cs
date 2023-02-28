using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace TechInterviewer.Setup.Healthcheck;

public static class Health
{
    public static void Setup(
        IServiceCollection services,
        IConfiguration configuration)
    {
        services
            .AddTransient<DatabaseHealthCheck>()
            .AddHealthChecks()
            .AddCheck<DatabaseHealthCheck>("Database");
    }
}