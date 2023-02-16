namespace Domain.Services.Organizations;

public record LeaveOrganizationRequest
{
    public long? NewManagerId { get; init; }
}