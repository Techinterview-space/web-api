using System;

namespace Web.Api.Features.Interviews.Models;

public record InterviewUpdateRequest : InterviewCreateRequest
{
    public Guid Id { get; init; }
}