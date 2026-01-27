namespace Domain.ValueObjects.Ranges;

public record IntRange : CustomRange<int>
{
    public IntRange(
        int start, int end)
        : base(start, end)
    {
    }
}