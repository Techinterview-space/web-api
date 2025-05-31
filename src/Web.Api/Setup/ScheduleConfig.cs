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
            .AddTransient<StatDataChangeSubscriptionCalculateJob>()
            .AddTransient<AiAnalysisSubscriptionJob>()
            .AddTransient<UserUniqueTokenSetJob>()
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
                .Schedule<SalaryUpdateReminderEmailJob>()
                .DailyAt(0, 30)
                .RunOnceAtStart();

            scheduler
                .Schedule<UserUniqueTokenSetJob>()
                .EveryMinute()
                .PreventOverlapping(nameof(UserUniqueTokenSetJob));

            scheduler
                .Schedule<TelegramSalariesRegularStatsUpdateJob>()
                .DailyAt(7, 0)
                .RunOnceAtStart();

            scheduler
                .Schedule<StatDataChangeSubscriptionCalculateJob>()
                .DailyAt(6, 0)
                .Wednesday()
                .PreventOverlapping(nameof(StatDataChangeSubscriptionCalculateJob));

            // this job should go before StatDataChangeSubscriptionCalculateJob
            scheduler
                .Schedule<AiAnalysisSubscriptionJob>()
                .DailyAt(5, 0)
                .Wednesday()
                .PreventOverlapping(nameof(AiAnalysisSubscriptionJob));

            scheduler
                .Schedule<StatDataChangeSubscriptionCalculateJob>()
                .EveryMinute()
                .When(() => Task.FromResult(Debugger.IsAttached))
                .PreventOverlapping(nameof(StatDataChangeSubscriptionCalculateJob));
        });
    }
}