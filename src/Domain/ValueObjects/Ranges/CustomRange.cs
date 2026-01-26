namespace Domain.ValueObjects.Ranges;

public record CustomRange<T>
    where T : struct
{
    public CustomRange(
        T start,
        T end)
    {
        Start = start;
        End = end;
    }

    public T Start { get; }

    public T End { get; }
}