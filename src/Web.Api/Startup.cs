using Domain.Database;
using Domain.Enums;
using Domain.Services.Global;
using MaximGorbatyuk.DatabaseSqlEndpoints;
using MG.Utils.Abstract.NonNullableObjects;
using MG.Utils.AspNetCore.HealthCheck;
using MG.Utils.AspNetCore.Middlewares;
using MG.Utils.AspNetCore.Swagger;
using MG.Utils.SerilogElk.Builders;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Logging;
using Serilog;
using TechInterviewer.Setup;
using TechInterviewer.Setup.Healthcheck;

namespace TechInterviewer;

public class Startup
{
    private const string CorsPolicyName = "CorsPolicy";

    private readonly NonNullableString _appName = new ("Tech.Interview.API");
    private readonly IGlobal _global;

    public Startup(IConfiguration configuration, IHostEnvironment environment)
    {
        Configuration = configuration;
        Environment = environment;
        _global = new Global(configuration);
    }

    public IConfiguration Configuration { get; }

    public IHostEnvironment Environment { get; }

    // This method gets called by the runtime. Use this method to add services to the container.
    public void ConfigureServices(IServiceCollection services)
    {
        new ElkSerilog(
            config: Configuration,
            appName: _appName,
            connectionString: new NonNullableString(Configuration.GetConnectionString("Elasticsearch")),
            environmentName: new NonNullableString(Environment.EnvironmentName)).Setup();

        services.AddControllersWithViews();
        services.AddRazorPages();

        services.AddSwaggerGen(c => SwaggerConfig.Apply(c, _appName, new NonNullableString(_global.FrontendBaseUrl)));

        services.AddCors(options =>
        {
            options.AddPolicy(CorsPolicyName, builder => builder
                .AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader());
        });

        DatabaseConfig.Setup(services, Configuration, Environment);
        ServiceRegistration.Setup(services, Configuration);
        ServiceRegistration.EmailIntegration(services, Environment);
        Health.Setup(services, Configuration);
        Auth.Setup(services, Configuration);
        ScheduleConfig.Setup(services);

        services.AddHostedService<AppInitializeService>();
    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    public void Configure(IApplicationBuilder app, ILoggerFactory loggerFactory)
    {
        if (Environment.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
            IdentityModelEventSource.ShowPII = true;
        }

        loggerFactory.AddSerilog();

        app
            .UseMiddleware<ExceptionHttpMiddleware>()
            .UseLoggingMiddleware();

        app.UseSwagger();
        app.UseSwaggerUI(c => SwaggerConfig.ApplyUI(c, _appName));

        app.UseHttpsRedirection();
        app.UseRouting();
        app.UseCors(CorsPolicyName);

        app.UseAuthentication();
        app.UseAuthorization();

        HealthCheckCustomResponse.Setup(app);
        ScheduleConfig.Use(app);

        app
            .UseDatabaseTable<DatabaseContext>(checkForAuthentication: true, sqlEngine: SqlEngine.PostgreSQL, roleToCheckForAuthorization: Role.Admin.ToString())
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