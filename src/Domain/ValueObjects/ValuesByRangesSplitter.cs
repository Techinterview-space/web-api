using System;
using System.Collections;
using System.Collections.Generic;
using Domain.ValueObjects.Ranges;

namespace Domain.ValueObjects;

public record ValuesByRangesSplitter : IEnumerable<DoublesRange>
{
    private readonly double _min;
    private readonly double _max;
    private readonly int _step;

    private List<DoublesRange> _ranges;

    public ValuesByRangesSplitter(
        double min,
        double max,
        int step)
    {
        _min = min;
        _max = max;
        _step = step;
    }

    public IEnumerator<DoublesRange> GetEnumerator()
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

    public List<DoublesRange> ToList()
    {
        if (_ranges != null)
        {
            return _ranges;
        }

        _ranges = PrepareData();
        return _ranges;
    }

    private List<DoublesRange> PrepareData()
    {
        var ranges = new List<DoublesRange>();
        var start = _min;
        var end = start + _step;
        while (end <= _max)
        {
            ranges.Add(new DoublesRange(start, end));
            start = end;
            end += _step;
        }

        if (Math.Abs(start - _max) > 0.01)
        {
            ranges.Add(new DoublesRange(start, _max));
        }

        return ranges;
    }
}