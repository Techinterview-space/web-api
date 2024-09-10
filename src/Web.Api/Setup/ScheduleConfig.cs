using System.Diagnostics;
using System.Threading.Tasks;
using Coravel;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Web.Api.Features.BackgroundJobs;

namespace Web.Api.Setup;

public static class ScheduleConfig
{
    public static IServiceCollection SetupScheduler(
        this IServiceCollection services)
    {
        services
            .AddScheduler()
            .AddTransient<CurrenciesResetJob>()
            .AddTransient<TelegramSalariesRegularStatsUpdateJob>()
            .AddTransient<StatDataCacheItemsCreateJob>();

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

            scheduler
                .Schedule<TelegramSalariesRegularStatsUpdateJob>()
                .DailyAt(7, 0)
                .RunOnceAtStart();

            scheduler
                .Schedule<StatDataCacheItemsCreateJob>()
                .DailyAt(6, 0)
                .Wednesday();

            scheduler
                .Schedule<StatDataCacheItemsCreateJob>()
                .EveryThirtySeconds()
                .When(() => Task.FromResult(Debugger.IsAttached));
        });
    }
}