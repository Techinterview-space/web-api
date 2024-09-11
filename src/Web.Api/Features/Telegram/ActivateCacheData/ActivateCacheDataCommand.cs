using System;
using MediatR;

namespace Web.Api.Features.Telegram.ActivateCacheData;

public record ActivateCacheDataCommand : IRequest<Unit>
{
    public ActivateCacheDataCommand(
        Guid id)
    {
        Id = id;
    }

    public Guid Id { get; }
}