using System;

namespace MG.Utils.Abstract.Dates.Interfaces
{
    public interface IHasFromToDates
    {
        DateTimeOffset From { get; set; }

        DateTimeOffset? To { get; set; }
    }
}