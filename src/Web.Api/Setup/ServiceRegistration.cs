using AspNetCore.Aws.S3.Simple.Settings;
using Domain.Authentication;
using Domain.Authentication.Abstract;
using Domain.Emails.Services;
using Domain.Files;
using Domain.Services.Global;
using Domain.Services.Html;
using Domain.Services.Interviews;
using Infrastructure.Emails;
using Infrastructure.Services.Files;
using Infrastructure.Services.Http;
using Infrastructure.Services.PDF;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace TechInterviewer.Setup;

public static class ServiceRegistration
{
    public static IServiceCollection SetupAppServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddHttpContextAccessor();
        services.AddScoped<IHttpContext, AppHttpContext>();
        services.AddScoped<IAuthorization, Authorization>();
        services.AddScoped<IGlobal, Global>();
        services.AddScoped<ITechInterviewHtmlGenerator, TechInterviewHtmlGenerator>();

        services.AddScoped<IPdf, PdfRenderer>();

        // https://github.com/rdvojmoc/DinkToPdf/#dependency-injection
        services.AddSingleton<IDisposableConverter, InjectedSynchronizedConverter>();
        services.AddScoped<IInterviewPdf, InterviewPdf>();

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