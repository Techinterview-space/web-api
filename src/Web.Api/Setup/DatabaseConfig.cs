using System;
using Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Web.Api.Setup;

public static class DatabaseConfig
{
    public static IServiceCollection SetupDatabase(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        services
            .AddDbContext<DatabaseContext>(options =>
            {
                options.UseNpgsql(configuration.GetConnectionString("Database"));

                // https://github.com/zzzprojects/EntityFramework-Extensions/issues/441#issue-1014382709
                // https://stackoverflow.com/a/70304966
                AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
                options.IgnoreMultipleCollectionIncludeWarningWhen(!environment.IsDevelopment());

                // TODO mgorbatyuk: Without his line, the propject can't execute migrations
                // https://learn.microsoft.com/en-us/ef/core/what-is-new/ef-core-9.0/breaking-changes#mitigations
                // https://github.com/dotnet/efcore/issues/34431
                options.ConfigureWarnings(w => w.Ignore(RelationalEventId.PendingModelChangesWarning));
            });

        return services;
    }
}