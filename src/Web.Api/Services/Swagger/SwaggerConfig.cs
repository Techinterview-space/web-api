using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;
using Swashbuckle.AspNetCore.SwaggerUI;

namespace Web.Api.Services.Swagger;

public static class SwaggerConfig
{
    private const string Version = "v1";

    private const string Endpoint = "/swagger/v1/swagger.json";

    public static void ApplyUI(SwaggerUIOptions config, string appName)
    {
        config.RoutePrefix = string.Empty;
        config.SwaggerEndpoint(Endpoint, appName);
        config.DisplayRequestDuration();
        config.DocExpansion(DocExpansion.List);
    }

    public static void Apply(SwaggerGenOptions config, string appName, string frontendAppLink)
    {
        config.SwaggerDoc(Version, new OpenApiInfo
        {
            Title = appName,
            Version = Version,
            Description = $"Frontend: {frontendAppLink}",
        });

        const string bearer = "Bearer";

        // copied from https://stackoverflow.com/a/58972781
        config.AddSecurityDefinition(bearer, new OpenApiSecurityScheme
        {
            Description = @"JWT Authorization header using the Bearer scheme.",
            Name = "Authorization",
            In = ParameterLocation.Header,
            Type = SecuritySchemeType.ApiKey,
            Scheme = bearer
        });
    }
}
