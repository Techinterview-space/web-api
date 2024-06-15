using System;
using Domain.Entities.Interviews;

namespace Web.Api.Features.Interviews.RevokeShareLinkToken;

public record GetShareLinkTokenResult
{
    public Guid Token { get; }

    public DateTimeOffset RevokedAt { get; }

    public GetShareLinkTokenResult(
        ShareLink shareLink)
    {
        if (!shareLink.ShareToken.HasValue)
        {
            throw new InvalidOperationException("ShareToken is not set.");
        }

        Token = shareLink.ShareToken.Value;
        RevokedAt = shareLink.UpdatedAt;
    }
}