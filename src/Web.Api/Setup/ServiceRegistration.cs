using AspNetCore.Aws.S3.Simple.Settings;
using Domain.Authentication;
using Domain.Authentication.Abstract;
using Domain.Emails.Services;
using Domain.Services.Global;
using Domain.Services.Html;
using Domain.Services.Interviews;
using EmailService.Integration.Core;
using EmailService.Integration.Core.Clients;
using FileService.Contracts;
using FileService.Implementation;
using MG.Utils.AspNetCore.Views;
using MG.Utils.Export.Pdf;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace TechInterviewer.Setup;

public static class ServiceRegistration
{
    public static void Setup(IServiceCollection services, IConfiguration configuration)
    {
        services.AddHttpContextAccessor();
        services.AddScoped<IHttpContext, AppHttpContext>();
        services.AddScoped<IAuthorization, Authorization>();
        services.AddScoped<IView, ViewService>();
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
    }

    public static void EmailIntegration(
        IServiceCollection services,
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
    }
}