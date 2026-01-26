using System;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using Domain.Entities.Auth;
using Infrastructure.Authentication.OAuth;
using Infrastructure.Database;
using Infrastructure.Services.Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Web.Api.Features.Auth.Handlers;
using Web.Api.Features.Auth.Requests;
using Web.Api.Features.Auth.Responses;
using Web.Api.Setup.Attributes;

namespace Web.Api.Features.Auth;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IConfiguration _configuration;
    private readonly DatabaseContext _context;

    public AuthController(
        IServiceProvider serviceProvider,
        IConfiguration configuration,
        DatabaseContext context)
    {
        _serviceProvider = serviceProvider;
        _configuration = configuration;
        _context = context;
    }

    [HttpGet("google")]
    [AllowAnonymous]
    public IActionResult GoogleLogin([FromQuery] string redirectUri)
    {
        var googleProvider = HttpContext.RequestServices.GetRequiredService<GoogleOAuthProvider>();
        var state = GenerateState();

        HttpContext.Session.SetString("oauth_state", state);

        var authUrl = googleProvider.GetAuthorizationUrl(state, redirectUri);
        return Redirect(authUrl);
    }

    [HttpGet("google/callback")]
    [AllowAnonymous]
    public async Task<IActionResult> GoogleCallback(
        [FromQuery] string code,
        [FromQuery] string state,
        [FromServices] GoogleCallbackHandler handler,
        CancellationToken cancellationToken)
    {
        var storedState = HttpContext.Session.GetString("oauth_state");
        if (state != storedState)
        {
            return BadRequest("Invalid state parameter");
        }

        var request = new GoogleCallbackRequest
        {
            Code = code,
            DeviceInfo = Request.Headers["User-Agent"].ToString(),
        };
        var result = await handler.HandleAsync(request, cancellationToken);

        var frontendUrl = _configuration["Frontend:CallbackUrl"];
        return Redirect($"{frontendUrl}?access_token={result.AccessToken}&refresh_token={result.RefreshToken}");
    }

    [HttpGet("github")]
    [AllowAnonymous]
    public IActionResult GitHubLogin([FromQuery] string redirectUri)
    {
        var githubProvider = HttpContext.RequestServices.GetRequiredService<GitHubOAuthProvider>();
        var state = GenerateState();

        HttpContext.Session.SetString("oauth_state", state);

        var authUrl = githubProvider.GetAuthorizationUrl(state, redirectUri);
        return Redirect(authUrl);
    }

    [HttpGet("github/callback")]
    [AllowAnonymous]
    public async Task<IActionResult> GitHubCallback(
        [FromQuery] string code,
        [FromQuery] string state,
        [FromServices] GitHubCallbackHandler handler,
        CancellationToken cancellationToken)
    {
        var storedState = HttpContext.Session.GetString("oauth_state");
        if (state != storedState)
        {
            return BadRequest("Invalid state parameter");
        }

        var request = new GitHubCallbackRequest
        {
            Code = code,
            DeviceInfo = Request.Headers["User-Agent"].ToString(),
        };
        var result = await handler.HandleAsync(request, cancellationToken);

        var frontendUrl = _configuration["Frontend:CallbackUrl"];
        return Redirect($"{frontendUrl}?access_token={result.AccessToken}&refresh_token={result.RefreshToken}");
    }

    [HttpPost("register")]
    [AllowAnonymous]
    public Task<AuthResult> Register(
        [FromBody] RegisterRequest request,
        CancellationToken cancellationToken)
    {
        return _serviceProvider.HandleBy<RegisterHandler, RegisterRequest, AuthResult>(
            request,
            cancellationToken);
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public Task<AuthTokenResponse> Login(
        [FromBody] LoginRequest request,
        CancellationToken cancellationToken)
    {
        return _serviceProvider.HandleBy<LoginHandler, LoginRequest, AuthTokenResponse>(
            request,
            cancellationToken);
    }

    [HttpGet("verify-email/{token}")]
    [AllowAnonymous]
    public async Task<IActionResult> VerifyEmail(
        [FromRoute] string token,
        [FromServices] VerifyEmailHandler handler,
        CancellationToken cancellationToken)
    {
        await handler.Handle(token, cancellationToken);

        var frontendUrl = _configuration["Frontend:BaseUrl"];
        return Redirect($"{frontendUrl}/login?verified=true");
    }

    [HttpPost("forgot-password")]
    [AllowAnonymous]
    public Task<AuthResult> ForgotPassword(
        [FromBody] ForgotPasswordRequest request,
        CancellationToken cancellationToken)
    {
        return _serviceProvider.HandleBy<ForgotPasswordHandler, ForgotPasswordRequest, AuthResult>(
            request,
            cancellationToken);
    }

    [HttpPost("reset-password")]
    [AllowAnonymous]
    public Task<AuthResult> ResetPassword(
        [FromBody] ResetPasswordRequest request,
        CancellationToken cancellationToken)
    {
        return _serviceProvider.HandleBy<ResetPasswordHandler, ResetPasswordRequest, AuthResult>(
            request,
            cancellationToken);
    }

    [HttpPost("resend-verification")]
    [AllowAnonymous]
    public Task<AuthResult> ResendVerification(
        [FromBody] ForgotPasswordRequest request,
        CancellationToken cancellationToken)
    {
        return _serviceProvider.HandleBy<ResendVerificationHandler, ForgotPasswordRequest, AuthResult>(
            request,
            cancellationToken);
    }

    [HttpPost("refresh")]
    [AllowAnonymous]
    public Task<AuthTokenResponse> RefreshToken(
        [FromBody] RefreshTokenRequest request,
        CancellationToken cancellationToken)
    {
        return _serviceProvider.HandleBy<RefreshTokenHandler, RefreshTokenRequest, AuthTokenResponse>(
            request,
            cancellationToken);
    }

    [HttpPost("m2m/token")]
    [AllowAnonymous]
    public async Task<M2mTokenResponse> M2mToken(
        [FromBody] M2mTokenRequest request,
        [FromServices] M2mTokenHandler handler,
        CancellationToken cancellationToken)
    {
        request.IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
        return await handler.Handle(request, cancellationToken);
    }

    [HttpPost("logout")]
    [HasAnyRole]
    public async Task<IActionResult> Logout(
        [FromBody] LogoutRequest request,
        CancellationToken cancellationToken)
    {
        if (!string.IsNullOrEmpty(request.RefreshToken))
        {
            var token = await _context.Set<RefreshToken>()
                .FirstOrDefaultAsync(rt => rt.Token == request.RefreshToken, cancellationToken);

            if (token != null)
            {
                token.Revoke();
                await _context.TrySaveChangesAsync(cancellationToken);
            }
        }

        return Ok();
    }

    private static string GenerateState()
    {
        var bytes = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(bytes);
        return Convert.ToBase64String(bytes);
    }
}
