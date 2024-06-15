using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Web.Api.Setup.Healthcheck;

public class HealthCheckCustomResponse : HealthCheckOptions
{
    public static void Setup(IApplicationBuilder app)
    {
        const string healthCheckRoute = "/health";

        app.UseHealthChecks(healthCheckRoute, new HealthCheckCustomResponse());
    }

    public HealthCheckCustomResponse()
    {
        ResponseWriter = WriteAsync;
    }

    private static async Task WriteAsync(HttpContext context, HealthReport report)
    {
        context.Response.ContentType = "application/json";

        var json = System.Text.Json.JsonSerializer.Serialize(new
        {
            status = report.Status.ToString(),
            errors = report.Entries.Select(e => new { key = e.Key, value = e.Value.Status.ToString() })
        });

        await context.Response.WriteAsync(json);
    }
}