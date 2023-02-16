using Shared.Enums;

namespace Domain.Enums;

public interface IHasStatus
{
    Status Status { get; }
}