using System;

namespace Domain.Services.Interviews.Dtos;

public record InterviewUpdateRequest : InterviewCreateRequest
{
    public Guid Id { get; init; }
}