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
            .AddTransient<SalariesSubscriptionCalculateJob>()
            .AddTransient<SalariesAiAnalysisSubscriptionWeeklyJob>()
            .AddTransient<SalaryUpdateReminderEmailJob>();

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
                .Schedule<SalariesSubscriptionCalculateJob>()
                .DailyAt(6, 0)
                .Wednesday()
                .PreventOverlapping(nameof(SalariesSubscriptionCalculateJob));

            // this job should go before StatDataChangeSubscriptionCalculateJob
            scheduler
                .Schedule<SalariesAiAnalysisSubscriptionWeeklyJob>()
                .DailyAt(5, 0)
                .Wednesday()
                .PreventOverlapping(nameof(SalariesAiAnalysisSubscriptionWeeklyJob));

            scheduler
                .Schedule<SalariesSubscriptionCalculateJob>()
                .EveryMinute()
                .When(() => Task.FromResult(Debugger.IsAttached))
                .PreventOverlapping(nameof(SalariesSubscriptionCalculateJob));

            for (var i = 0; i < 10; i++)
            {
                scheduler
                    .Schedule<SalaryUpdateReminderEmailJob>()
                    .DailyAt(i * 1, 30);
            }
        });
    }
}