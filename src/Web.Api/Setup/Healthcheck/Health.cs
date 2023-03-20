﻿using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace TechInterviewer.Setup.Healthcheck;

public static class Health
{
    public static IServiceCollection SetupHealthCheck(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services
            .AddTransient<DatabaseHealthCheck>()
            .AddHealthChecks()
            .AddCheck<DatabaseHealthCheck>("Database");

        return services;
    }
}