using Microsoft.AspNetCore.Builder;

namespace Web.Api.Middlewares;

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