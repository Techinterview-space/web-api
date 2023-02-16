using System;

namespace Domain.Services.Organizations;

public record UpdateOrganizationRequest : CreateOrganizationRequest
{
    public Guid Id { get; init; }
}