namespace Domain.ValueObjects.Dates.Interfaces
{
    public interface IHasTimeRange : IHasFromToDates
    {
        TimeRange Range() => new (From, To);
    }
}