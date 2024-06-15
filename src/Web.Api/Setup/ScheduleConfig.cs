using Coravel;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Web.Api.Features.BackgroundJobs;

namespace Web.Api.Setup;

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
            .AddScheduler()
            .AddTransient<CurrenciesResetJob>();

        return services;
    }

    public static void Use(IApplicationBuilder app)
    {
        app.ApplicationServices.UseScheduler((scheduler) =>
        {
            scheduler
                .Schedule<CurrenciesResetJob>()
                .DailyAt(6, 0)
                .RunOnceAtStart();
        });
    }
}