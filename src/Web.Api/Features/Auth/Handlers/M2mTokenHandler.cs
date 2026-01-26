using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Domain.Validation.Exceptions;
using Infrastructure.Authentication;
using Infrastructure.Jwt;
using Infrastructure.Services.Mediator;
using Web.Api.Features.Auth.Requests;
using Web.Api.Features.Auth.Responses;

namespace Web.Api.Features.Auth.Handlers;

public class M2mTokenHandler : IRequestHandler<M2mTokenRequest, M2mTokenResponse>
{
    private readonly IM2mClientService _m2mClientService;
    private readonly IJwtTokenService _jwtTokenService;

    public M2mTokenHandler(
        IM2mClientService m2mClientService,
        IJwtTokenService jwtTokenService)
    {
        _m2mClientService = m2mClientService;
        _jwtTokenService = jwtTokenService;
    }

    public async Task<M2mTokenResponse> Handle(
        M2mTokenRequest request,
        CancellationToken cancellationToken)
    {
        var client = await _m2mClientService.ValidateClientCredentialsAsync(
            request.ClientId,
            request.ClientSecret);

        if (client == null)
        {
            throw new UnauthorizedException("Invalid client credentials");
        }

        await _m2mClientService.RecordUsageAsync(
            client.ClientId,
            request.IpAddress);

        var accessToken = _jwtTokenService.GenerateAccessToken(
            client,
            request.Scopes ?? Array.Empty<string>());

        var grantedScopes = client.Scopes
            .Select(s => s.Scope)
            .Where(s => request.Scopes == null ||
                        request.Scopes.Length == 0 ||
                        request.Scopes.Contains(s))
            .ToArray();

        return new M2mTokenResponse
        {
            AccessToken = accessToken,
            TokenType = "Bearer",
            ExpiresIn = 3600,
            Scopes = grantedScopes,
        };
    }
}
