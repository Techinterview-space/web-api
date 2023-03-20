using Domain.ValueObjects.Dates.Interfaces;

namespace Domain.Entities;

// TODO Maxim: try to remove
public interface IBaseModel : IHasId, IHasDates
{
}