using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace Web.Api.Features.WellKnown;

/// <summary>
/// Serves agent-discovery metadata that the .NET API is the authoritative source for.
/// The OAuth authorization-server document (RFC 8414) lives on the frontend because the
/// JWT issuer is the frontend origin; only the protected-resource document (RFC 9728)
/// is served here.
/// </summary>
[ApiController]
[AllowAnonymous]
public class WellKnownController : ControllerBase
{
    private readonly IConfiguration _configuration;

    public WellKnownController(
        IConfiguration configuration)
    {
        _configuration = configuration;
    }

    [HttpGet("/.well-known/oauth-protected-resource")]
    [Produces("application/json")]
    [ResponseCache(Duration = 3600)]
    public ActionResult<OAuthProtectedResourceMetadata> GetProtectedResourceMetadata()
    {
        var authorizationServer = _configuration["OAuth:Jwt:Issuer"];
        var resource = $"{Request.Scheme}://{Request.Host}";

        return Ok(new OAuthProtectedResourceMetadata
        {
            Resource = resource,
            AuthorizationServers = string.IsNullOrEmpty(authorizationServer)
                ? null
                : new[] { authorizationServer },
            BearerMethodsSupported = new[] { "header" },
            ResourceDocumentation = $"{resource}/swagger",
        });
    }
}

public sealed record OAuthProtectedResourceMetadata
{
    [JsonPropertyName("resource")]
    public string Resource { get; init; }

    [JsonPropertyName("authorization_servers")]
    public string[] AuthorizationServers { get; init; }

    [JsonPropertyName("bearer_methods_supported")]
    public string[] BearerMethodsSupported { get; init; }

    [JsonPropertyName("resource_documentation")]
    public string ResourceDocumentation { get; init; }
}
