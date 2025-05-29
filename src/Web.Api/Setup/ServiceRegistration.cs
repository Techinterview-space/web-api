using System;
using System.Net.Http.Headers;
using System.Text;
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
        IHostEnvironment environment,
        IConfiguration configuration)
    {
        services
            .AddScoped<ITechinterviewEmailService, TechInterviewerEmailService>();

        if (environment.IsDevelopment())
        {
            services
                .AddScoped<IEmailApiSender, LocalEmailApiSender>();
        }
        else
        {
            services
                .AddScoped<IEmailApiSender, MailgunEmailSender>();

            var mailgunApiKey = configuration["MailgunApiKey"];
            if (string.IsNullOrWhiteSpace(mailgunApiKey))
            {
                throw new InvalidOperationException("Mailgun API key is not configured.");
            }

            var mailgunDomain = configuration["MailgunDomain"];
            if (string.IsNullOrWhiteSpace(mailgunDomain))
            {
                throw new InvalidOperationException("Mailgun domain is not configured.");
            }

            services.AddHttpClient<IEmailApiSender, MailgunEmailSender>(c =>
            {
                c.BaseAddress = new Uri($"https://api.mailgun.net/v3/{mailgunDomain}/messages");
                c.DefaultRequestHeaders.Add("Accept", "application/json");
                c.DefaultRequestHeaders.Add(
                    "Authorization",
                    "Basic " + Convert.ToBase64String(
                        Encoding.UTF8.GetBytes($"api:{mailgunApiKey}")));
            });
        }

        return services;
    }
}