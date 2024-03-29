﻿using Coravel;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace TechInterviewer.Setup;

public static class ScheduleConfig
{
    private const int Night = 0;
    private const int EarlyMorning = 3;
    private const int Midday = 9;
    private const int Afternoon = 15;
    private const int Evening = 21;

    public static IServiceCollection SetupScheduler(
        this IServiceCollection services)
    {
        services
            .AddScheduler();

        return services;
    }

    public static void Use(IApplicationBuilder app)
    {
        app.ApplicationServices.UseScheduler((scheduler) =>
        {
        });
    }
}