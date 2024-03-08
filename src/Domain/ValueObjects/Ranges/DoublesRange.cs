namespace Domain.ValueObjects.Ranges;

public record DoublesRange : CustomRange<double>
{
    public DoublesRange(
        double start, double end)
        : base(start, end)
    {
    }
}