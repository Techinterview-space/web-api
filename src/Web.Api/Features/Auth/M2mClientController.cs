using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Domain.Enums;
using Infrastructure.Authentication;
using Infrastructure.Authentication.Contracts;
using Microsoft.AspNetCore.Mvc;
using Web.Api.Setup.Attributes;

namespace Web.Api.Features.Auth;

[ApiController]
[Route("api/m2m-clients")]
[HasAnyRole(Role.Admin)]
public class M2mClientController : ControllerBase
{
    private readonly IM2mClientService _m2mClientService;
    private readonly IAuthorization _auth;

    public M2mClientController(
        IM2mClientService m2mClientService,
        IAuthorization auth)
    {
        _m2mClientService = m2mClientService;
        _auth = auth;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var clients = await _m2mClientService.GetAllAsync();
        return Ok(clients.Select(c => new M2mClientDto
        {
            Id = c.Id,
            Name = c.Name,
            Description = c.Description,
            ClientId = c.ClientId,
            IsActive = c.IsActive,
            Scopes = c.Scopes.Select(s => s.Scope).ToArray(),
            CreatedByUserEmail = c.CreatedByUser?.Email,
            LastUsedAt = c.LastUsedAt,
            LastUsedIpAddress = c.LastUsedIpAddress,
            RateLimitPerMinute = c.RateLimitPerMinute,
            RateLimitPerDay = c.RateLimitPerDay,
            CreatedAt = c.CreatedAt,
        }));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById([FromRoute] long id)
    {
        var client = await _m2mClientService.GetByIdAsync(id);
        if (client == null)
        {
            return NotFound();
        }

        return Ok(new M2mClientDto
        {
            Id = client.Id,
            Name = client.Name,
            Description = client.Description,
            ClientId = client.ClientId,
            IsActive = client.IsActive,
            Scopes = client.Scopes.Select(s => s.Scope).ToArray(),
            CreatedByUserEmail = client.CreatedByUser?.Email,
            LastUsedAt = client.LastUsedAt,
            LastUsedIpAddress = client.LastUsedIpAddress,
            RateLimitPerMinute = client.RateLimitPerMinute,
            RateLimitPerDay = client.RateLimitPerDay,
            CreatedAt = client.CreatedAt,
        });
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateM2mClientRequest request, CancellationToken cancellationToken)
    {
        var currentUser = await _auth.GetCurrentUserOrFailAsync(cancellationToken);

        var (client, plainSecret) = await _m2mClientService.CreateClientAsync(
            request.Name,
            request.Description,
            request.Scopes,
            currentUser.Id);

        return Ok(new M2mClientCreatedDto
        {
            Id = client.Id,
            Name = client.Name,
            ClientId = client.ClientId,
            ClientSecret = plainSecret,
            Scopes = request.Scopes,
            CreatedAt = client.CreatedAt,
        });
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update([FromRoute] long id, [FromBody] UpdateM2mClientRequest request)
    {
        var client = await _m2mClientService.GetByIdAsync(id);
        if (client == null)
        {
            return NotFound();
        }

        client.Update(request.Name, request.Description, request.RateLimitPerMinute, request.RateLimitPerDay);

        return Ok(new { message = "Client updated successfully" });
    }

    [HttpPost("{id}/regenerate-secret")]
    public async Task<IActionResult> RegenerateSecret([FromRoute] long id)
    {
        var newSecret = await _m2mClientService.RegenerateSecretAsync(id);
        return Ok(new { clientSecret = newSecret });
    }

    [HttpPut("{id}/scopes")]
    public async Task<IActionResult> UpdateScopes([FromRoute] long id, [FromBody] UpdateScopesRequest request)
    {
        await _m2mClientService.UpdateScopesAsync(id, request.Scopes);
        return Ok(new { message = "Scopes updated successfully" });
    }

    [HttpPost("{id}/deactivate")]
    public async Task<IActionResult> Deactivate([FromRoute] long id)
    {
        await _m2mClientService.DeactivateAsync(id);
        return Ok(new { message = "Client deactivated successfully" });
    }

    [HttpPost("{id}/activate")]
    public async Task<IActionResult> Activate([FromRoute] long id)
    {
        await _m2mClientService.ActivateAsync(id);
        return Ok(new { message = "Client activated successfully" });
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete([FromRoute] long id)
    {
        await _m2mClientService.DeleteAsync(id);
        return Ok(new { message = "Client deleted successfully" });
    }

    [HttpGet("scopes")]
    public IActionResult GetAvailableScopes()
    {
        return Ok(M2mScope.AllScopes);
    }
}

public record CreateM2mClientRequest
{
    public string Name { get; init; }

    public string Description { get; init; }

    public string[] Scopes { get; init; }

    public int? RateLimitPerMinute { get; init; }

    public int? RateLimitPerDay { get; init; }
}

public record UpdateM2mClientRequest
{
    public string Name { get; init; }

    public string Description { get; init; }

    public int? RateLimitPerMinute { get; init; }

    public int? RateLimitPerDay { get; init; }
}

public record UpdateScopesRequest
{
    public string[] Scopes { get; init; }
}

public record M2mClientDto
{
    public long Id { get; init; }

    public string Name { get; init; }

    public string Description { get; init; }

    public string ClientId { get; init; }

    public bool IsActive { get; init; }

    public string[] Scopes { get; init; }

    public string CreatedByUserEmail { get; init; }

    public DateTimeOffset? LastUsedAt { get; init; }

    public string LastUsedIpAddress { get; init; }

    public int? RateLimitPerMinute { get; init; }

    public int? RateLimitPerDay { get; init; }

    public DateTimeOffset CreatedAt { get; init; }
}

public record M2mClientCreatedDto
{
    public long Id { get; init; }

    public string Name { get; init; }

    public string ClientId { get; init; }

    public string ClientSecret { get; init; }

    public string[] Scopes { get; init; }

    public DateTimeOffset CreatedAt { get; init; }
}
