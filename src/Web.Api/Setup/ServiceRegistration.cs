using AspNetCore.Aws.S3.Simple.Settings;
using Infrastructure.Authentication;
using Infrastructure.Authentication.Contracts;
using Infrastructure.Currencies;
using Infrastructure.Currencies.Contracts;
using Infrastructure.Emails;
using Infrastructure.Emails.Contracts;
using Infrastructure.Services.Files;
using Infrastructure.Services.Global;
using Infrastructure.Services.Html;
using Infrastructure.Services.Http;
using Infrastructure.Services.PDF;
using Infrastructure.Services.PDF.Interviews;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Web.Api.Features.Salaries.Providers;
using Web.Api.Features.Telegram;

namespace Web.Api.Setup;

public static class ServiceRegistration
{
    public static IServiceCollection SetupAppServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddHttpContextAccessor();
        services.AddScoped<IHttpContext, AppHttpContext>();
        services.AddScoped<IAuthorization, AuthorizationService>();
        services.AddScoped<IGlobal, Global>();
        services.AddScoped<ITechInterviewHtmlGenerator, TechInterviewHtmlGenerator>();
        services.AddScoped<IPdf, PdfRenderer>();
        services.AddScoped<ISalaryLabelsProvider, SalaryLabelsProvider>();
        services.AddTransient<TelegramBotService>();
        services.AddTransient<ICurrencyService, CurrencyService>();

        // https://github.com/rdvojmoc/DinkToPdf/#dependency-injection
        services.AddSingleton<IDisposableConverter, InjectedSynchronizedConverter>();
        services.AddScoped<IInterviewPdfService, InterviewPdfService>();

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
            .AddScoped<IEmailService, TechInterviewerEmailService>();

        if (environment.IsDevelopment())
        {
            services
                .AddScoped<IEmailSender, LocalEmailSender>();
        }
        else
        {
            services
                .AddScoped<IEmailSender, SendGridEmailSender>();
        }

        return services;
    }
}