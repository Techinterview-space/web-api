using Domain.Database;
using Domain.Enums;
using Domain.Services.Global;
using MaximGorbatyuk.DatabaseSqlEndpoints;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Logging;
using Serilog;
using TechInterviewer.Middlewares;
using TechInterviewer.Services.Logging;
using TechInterviewer.Services.Swagger;
using TechInterviewer.Setup;
using TechInterviewer.Setup.Healthcheck;

namespace TechInterviewer;

public class Startup
{
    private const string CorsPolicyName = "CorsPolicy";

    private const string _appName = "Tech.Interview.API";
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
            connectionString: Configuration.GetConnectionString("Elasticsearch"),
            environmentName: Environment.EnvironmentName).Setup();

        services.AddControllersWithViews();
        services.AddRazorPages();

        services.AddSwaggerGen(c => SwaggerConfig.Apply(c, _appName, _global.FrontendBaseUrl));

        services.AddCors(options =>
        {
            options.AddPolicy(CorsPolicyName, builder => builder
                .AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader());
        });

        services
            .SetupDatabase(Configuration, Environment)
            .SetupAppServices(Configuration)
            .SetupEmailIntegration(Environment)
            .SetupHealthCheck(Configuration)
            .SetupAuthentication(Configuration)
            .SetupScheduler();

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
            .UseSqlEndpoints<DatabaseContext>(
                checkForAuthentication: true,
                roleToCheckForAuthorization: Role.Admin.ToString(),
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