using System.Collections.Generic;
using MG.Utils.Entities;

namespace Domain.Entities.Labels;

public abstract class HasLabelsEntity<T, TLabel> : HasDatesBase
where T : class
where TLabel : class
{
    public virtual ICollection<TLabel> Labels { get; protected set; } = new List<TLabel>();

    public T Sync(
        IReadOnlyCollection<TLabel> labelsFromRequest)
    {
        Labels.Clear();
        foreach (var label in labelsFromRequest)
        {
            Labels.Add(label);
        }

        return this as T;
    }
}