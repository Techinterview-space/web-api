﻿using Domain.Enums;
using Infrastructure.Configs;
using Infrastructure.Database;
using Infrastructure.Services.Global;
using MaximGorbatyuk.DatabaseSqlEndpoints;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Logging;
using Serilog;
using Web.Api.Middlewares;
using Web.Api.Services.Logging;
using Web.Api.Services.Swagger;
using Web.Api.Setup;
using Web.Api.Setup.Healthcheck;
using Web.Api.Setup.HostedServices;

namespace Web.Api;

public class Startup
{
    private const string CorsPolicyName = "CorsPolicy";
    private const string AppName = "TechInterview.API";

    private readonly IGlobal _global;
    private readonly IConfiguration _configuration;
    private readonly IHostEnvironment _environment;

    public Startup(
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        _configuration = configuration;
        _environment = environment;
        _global = new Global(configuration);
    }

    // This method gets called by the runtime. Use this method to add services to the container.
    public void ConfigureServices(IServiceCollection services)
    {
        DotEnvConfig.LoadEnvFileIfExists();
        new ElkSerilog(
            config: _configuration,
            appName: AppName,
            connectionString: _configuration.GetConnectionString("Elasticsearch"),
            environmentName: _environment.EnvironmentName).Setup();

        services.AddHttpClient();
        services.AddControllersWithViews();
        services.AddRazorPages();

        services
            .AddSwaggerGen(c => SwaggerConfig.Apply(c, AppName, _global.FrontendBaseUrl));

        services.AddCors(options =>
        {
            options.AddPolicy(CorsPolicyName, builder => builder
                .AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader());
        });

        services
            .SetupDatabase(_configuration, _environment)
            .SetupAppServices(_configuration)
            .SetupEmailIntegration(_environment, _configuration)
            .SetupHealthCheck(_configuration)
            .SetupAuthentication(_configuration)
            .SetupScheduler()
            .RegisterAllImplementations(
                typeof(Infrastructure.Services.Mediator.IRequestHandler<,>));

        services
            .AddHostedService<AppInitializeService>();
    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    public void Configure(
        IApplicationBuilder app,
        ILoggerFactory loggerFactory)
    {
        if (_environment.IsDevelopment())
        {
            app.UseMiddleware<DevelopmentEnvironmentMiddleware>();
            app.UseDeveloperExceptionPage();
            IdentityModelEventSource.ShowPII = true;
        }

        loggerFactory.AddSerilog();

        app
            .UseMiddleware<CorrelationIdMiddleware>()
            .UseMiddleware<ExceptionHttpMiddleware>()
            .UseLoggingMiddleware();

        app.UseSwagger();
        app.UseSwaggerUI(c => SwaggerConfig.ApplyUI(c, AppName));

        app.UseHttpsRedirection();
        app.UseRouting();
        app.UseCors(CorsPolicyName);

        app.UseAuthentication();
        app.UseAuthorization();

        HealthCheckCustomResponse.Setup(app);
        ScheduleConfig.Use(app);

        app
            .UseSqlEndpoints<DatabaseContext>(
                checkForAuthentication: true,
                roleToCheckForAuthorization: nameof(Role.Admin),
                sqlEngine: SqlEngine.PostgreSQL)
            .UseTableOutputEndpoint()
            .UseExecuteEndpoint()
            .UseReadEndpoint();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
        });

        app.UseMiddleware<DefaultNotFoundPageMiddleware>();
    }
}