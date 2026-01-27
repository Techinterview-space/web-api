using System;
using System.Linq;
using AspNetCore.Aws.S3.Simple.Settings;
using Infrastructure.Ai;
using Infrastructure.Ai.ChatGpt;
using Infrastructure.Ai.Claude;
using Infrastructure.Authentication;
using Infrastructure.Authentication.Contracts;
using Infrastructure.Authentication.OAuth;
using Infrastructure.Currencies;
using Infrastructure.Currencies.Contracts;
using Infrastructure.Emails;
using Infrastructure.Emails.Contracts;
using Infrastructure.Jwt;
using Infrastructure.Services.AiServices;
using Infrastructure.Services.AiServices.Custom;
using Infrastructure.Services.Correlation;
using Infrastructure.Services.Files;
using Infrastructure.Services.Github;
using Infrastructure.Services.Global;
using Infrastructure.Services.Html;
using Infrastructure.Services.Http;
using Infrastructure.Services.PDF;
using Infrastructure.Services.PDF.Interviews;
using Infrastructure.Services.Professions;
using Infrastructure.Services.Telegram.GithubProfile;
using Infrastructure.Services.Telegram.Notifications;
using Infrastructure.Services.Telegram.Salaries;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Web.Api.Features.Auth.Handlers;
using Web.Api.Features.Salaries.Providers;
using Web.Api.Features.Telegram;
using Web.Api.Services.CompanyReviews;
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
            .AddScoped<ICustomOpenAiService, CustomOpenAiService>()
            .AddTransient<ISalariesTelegramBotClientProvider, SalariesTelegramBotClientProvider>()
            .AddTransient<IGithubProfileBotProvider, GithubProfileBotProvider>()
            .AddTransient<SalariesTelegramBotHostedService>()
            .AddTransient<GithubProfileBotHostedService>()
            .AddTransient<ICurrencyService, CurrencyService>()
            .AddTransient<ICurrenciesHttpService, CurrenciesHttpService>()
            .AddTransient<IProfessionsCacheService, ProfessionsCacheService>()
            .AddTransient<SalariesSubscriptionService>()
            .AddTransient<CompanyReviewsSubscriptionService>()
            .AddScoped<ITelegramAdminNotificationService, TelegramAdminNotificationService>()
            .AddScoped<ICompanyReviewTelegramCallbackHandler, CompanyReviewTelegramCallbackHandler>()
            .AddScoped<IViewRenderer, ViewRenderer>()
            .AddScoped<IGithubPersonalUserTokenService, GithubPersonalUserTokenService>()
            .AddScoped<IGithubGraphQLService, GithubGraphQlService>()
            .AddScoped<IArtificialIntellectService, AiService>()
            .AddScoped<IAiProviderFactory, AiProviderFactory>()
            .AddScoped<ChatGptProvider>()
            .AddScoped<ClaudeAiProvider>()
            .AddScoped<IPasswordHasher, PasswordHasher>()
            .AddScoped<IJwtTokenService, JwtTokenService>()
            .AddScoped<IM2mClientService, M2mClientService>()
            .AddScoped<IOAuthProviderFactory, OAuthProviderFactory>()
            .AddScoped<GoogleOAuthProvider>()
            .AddScoped<GitHubOAuthProvider>()
            .AddScoped<GoogleCallbackHandler>()
            .AddScoped<GitHubCallbackHandler>();

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
                .AddScoped<IEmailApiSender, ResendEmailApiSender>();
        }

        return services;
    }

    public static IServiceCollection RegisterAllImplementations(
        this IServiceCollection services,
        Type openGenericType,
        ServiceLifetime lifetime = ServiceLifetime.Scoped)
    {
        var typesFromAssemblies = AppDomain.CurrentDomain
            .GetAssemblies()
            .Where(a => !a.IsDynamic)
            .SelectMany(a => a.GetTypes())
            .Where(t => !t.IsAbstract && !t.IsInterface)
            .Select(t => new
            {
                Implementation = t,
                Services = t.GetInterfaces()
                    .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == openGenericType)
            })
            .Where(x => x.Services.Any());

        foreach (var typeInfo in typesFromAssemblies)
        {
            foreach (var service in typeInfo.Services)
            {
                services.Add(new ServiceDescriptor(service, typeInfo.Implementation, lifetime));
                services.Add(new ServiceDescriptor(typeInfo.Implementation, typeInfo.Implementation, lifetime));
            }
        }

        return services;
    }
}