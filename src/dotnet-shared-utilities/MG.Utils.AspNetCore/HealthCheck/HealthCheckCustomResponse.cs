using System.Linq;
using System.Threading.Tasks;
using MG.Utils.Helpers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace MG.Utils.AspNetCore.HealthCheck
{
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

            await context.Response.WriteAsync(new
            {
                status = report.Status.ToString(),
                errors = report.Entries.Select(e => new { key = e.Key, value = e.Value.Status.ToString() })
            }.AsJson());
        }
    }
}