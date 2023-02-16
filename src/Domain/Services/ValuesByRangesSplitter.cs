using System;
using System.Collections;
using System.Collections.Generic;

namespace Domain.Services;

public record ValuesByRangesSplitter : IEnumerable<(double Start, double End)>
{
    private readonly double _min;
    private readonly double _max;
    private readonly int _step;

    private List<(double Start, double End)> _ranges;

    public ValuesByRangesSplitter(
        double min,
        double max,
        int step)
    {
        _min = min;
        _max = max;
        _step = step;
    }

    public IEnumerator<(double Start, double End)> GetEnumerator()
    {
        if (_ranges != null)
        {
            return _ranges.GetEnumerator();
        }

        _ranges = PrepareData();
        return _ranges.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public List<(double Start, double End)> ToList()
    {
        if (_ranges != null)
        {
            return _ranges;
        }

        _ranges = PrepareData();
        return _ranges;
    }

    private List<(double Start, double End)> PrepareData()
    {
        var ranges = new List<(double Start, double End)>();
        var start = _min;
        var end = start + _step;
        while (end <= _max)
        {
            ranges.Add((start, end));
            start = end;
            end += _step;
        }

        if (Math.Abs(start - _max) > 0.01)
        {
            ranges.Add((start, _max));
        }

        return ranges;
    }
}