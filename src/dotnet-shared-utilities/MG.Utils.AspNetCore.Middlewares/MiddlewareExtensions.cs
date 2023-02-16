using Microsoft.AspNetCore.Builder;

namespace MG.Utils.AspNetCore.Middlewares
{
    public static class MiddlewareExtensions
    {
        public static IApplicationBuilder UseLoggingMiddleware(this IApplicationBuilder app)
        {
            return app.UseMiddleware<LoggingMiddleware>();
        }

        public static IApplicationBuilder UseDefaultNotFoundPageMiddleware(this IApplicationBuilder app)
        {
            return app.UseMiddleware<DefaultNotFoundPageMiddleware>();
        }
    }
}