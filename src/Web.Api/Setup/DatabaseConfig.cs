using System;
using Domain.Database;
using MG.Utils.EFCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace TechInterviewer.Setup;

public static class DatabaseConfig
{
    public static void Setup(IServiceCollection services, IConfiguration configuration, IHostEnvironment environment)
    {
        services
            .AddDbContext<DatabaseContext>(options =>
            {
                options.UseNpgsql(configuration.GetConnectionString("Database"));

                // https://github.com/zzzprojects/EntityFramework-Extensions/issues/441#issue-1014382709
                // https://stackoverflow.com/a/70304966
                AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
                options.IgnoreMultipleCollectionIncludeWarningWhen(!environment.IsDevelopment());
            });
    }
}