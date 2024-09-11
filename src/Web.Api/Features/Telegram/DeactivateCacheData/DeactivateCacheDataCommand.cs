using System;
using MediatR;

namespace Web.Api.Features.Telegram.DeactivateCacheData;

public record DeactivateCacheDataCommand : IRequest<Unit>
{
    public DeactivateCacheDataCommand(
        Guid id)
    {
        Id = id;
    }

    public Guid Id { get; }
}