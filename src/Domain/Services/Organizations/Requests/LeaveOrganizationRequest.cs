namespace Domain.Services.Organizations.Requests;

public record LeaveOrganizationRequest
{
    public long? NewManagerId { get; init; }
}