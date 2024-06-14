using System;
using System.Collections;
using System.Collections.Generic;

namespace Domain.ValueObjects;

public record DateTimeRangeSplitter : IEnumerable<(DateTime Start, DateTime End)>
{
    private readonly DateTime _min;
    private readonly DateTime _max;
    private readonly TimeSpan _interval;

    private List<(DateTime Start, DateTime End)> _ranges;

    public DateTimeRangeSplitter(
        DateTime min,
        DateTime max,
        TimeSpan interval)
    {
        _min = min;
        _max = max;
        _interval = interval;
    }

    public IEnumerator<(DateTime Start, DateTime End)> GetEnumerator()
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

    public List<(DateTime Start, DateTime End)> ToList()
    {
        if (_ranges != null)
        {
            return _ranges;
        }

        _ranges = PrepareData();
        return _ranges;
    }

    private List<(DateTime Start, DateTime End)> PrepareData()
    {
        var ranges = new List<(DateTime Start, DateTime End)>();

        for (var time = _min; time < _max; time += _interval)
        {
            ranges.Add((time, time + _interval));
        }

        return ranges;
    }
}