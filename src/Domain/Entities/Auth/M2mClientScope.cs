using Domain.Validation;

namespace Domain.Entities.Auth;

public class M2mClientScope : BaseModel
{
    protected M2mClientScope()
    {
    }

    public M2mClientScope(
        long m2mClientId,
        string scope)
    {
        M2mClientId = m2mClientId;
        Scope = scope.ThrowIfNullOrEmpty(nameof(scope));
    }

    public long M2mClientId { get; protected set; }

    public virtual M2mClient M2mClient { get; protected set; }

    public string Scope { get; protected set; }
}
