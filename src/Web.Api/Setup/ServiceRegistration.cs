using AspNetCore.Aws.S3.Simple.Settings;
using Infrastructure.Authentication;
using Infrastructure.Authentication.Contracts;
using Infrastructure.Currencies;
using Infrastructure.Currencies.Contracts;
using Infrastructure.Emails;
using Infrastructure.Emails.Contracts;
using Infrastructure.Services.Correlation;
using Infrastructure.Services.Files;
using Infrastructure.Services.Global;
using Infrastructure.Services.Html;
using Infrastructure.Services.Http;
using Infrastructure.Services.OpenAi;
using Infrastructure.Services.PDF;
using Infrastructure.Services.PDF.Interviews;
using Infrastructure.Services.Professions;
using Infrastructure.Services.Telegram;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Web.Api.Features.Salaries.Providers;
using Web.Api.Features.Telegram;
using Web.Api.Services.Emails;
using Web.Api.Services.Salaries;
using Web.Api.Services.Views;

namespace Web.Api.Setup;

public static class ServiceRegistration
{
    public static IServiceCollection SetupAppServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddHttpContextAccessor()
            .AddTransient<ICorrelationIdAccessor, CorrelationIdAccessor>()
            .AddScoped<IHttpContext, AppHttpContext>()
            .AddScoped<IAuthorization, AuthorizationService>()
            .AddScoped<IGlobal, Global>()
            .AddScoped<IMarkdownToHtmlGenerator, MarkdownToHtmlGenerator>()
            .AddScoped<IPdf, QuestPdfBasedRender>()
            .AddScoped<ISalaryLabelsProvider, SalaryLabelsProvider>()
            .AddScoped<IOpenAiService, OpenAiService>()
            .AddTransient<ITelegramBotClientProvider, TelegramBotClientProvider>()
            .AddTransient<TelegramBotHostedService>()
            .AddTransient<ICurrencyService, CurrencyService>()
            .AddTransient<IProfessionsCacheService, ProfessionsCacheService>()
            .AddTransient<StatDataChangeSubscriptionService>()
            .AddScoped<ITelegramAdminNotificationService, TelegramNotificationService>()
            .AddScoped<IViewRenderer, ViewRenderer>();

        // https://github.com/rdvojmoc/DinkToPdf/#dependency-injection
        // services.AddSingleton<IDisposableConverter, InjectedSynchronizedConverter>();
        services.AddScoped<IInterviewPdfService, QuestPdfBasedService>();

        services
            .AddS3Settings()
            .AddS3Storage<ICvStorage, CvStorageS3Service>()
            .AddS3Storage<IPublicStorage, PublicStorage>();

        return services;
    }

    public static IServiceCollection SetupEmailIntegration(
        this IServiceCollection services,
        IHostEnvironment environment)
    {
        services
            .AddScoped<ITechinterviewEmailService, TechInterviewerEmailService>();

        if (environment.IsDevelopment())
        {
            services
                .AddScoped<ISendGridEmailSender, LocalEmailSender>();
        }
        else
        {
            services
                .AddScoped<ISendGridEmailSender, SendGridEmailSender>();
        }

        return services;
    }
}