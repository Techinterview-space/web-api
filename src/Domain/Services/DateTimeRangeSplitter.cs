﻿using System;
using System.Collections;
using System.Collections.Generic;

namespace Domain.Services;

public record DateTimeRangeSplitter : IEnumerable<(DateTime Start, DateTime End)>
{
    private readonly DateTime _min;
    private readonly DateTime _max;
    private readonly int _stepInMinutes;

    private List<(DateTime Start, DateTime End)> _ranges;

    public DateTimeRangeSplitter(
        DateTime min,
        DateTime max,
        int stepInMinutes)
    {
        _min = min;
        _max = max;
        _stepInMinutes = stepInMinutes;
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
        var interval = TimeSpan.FromMinutes(_stepInMinutes);
        var ranges = new List<(DateTime Start, DateTime End)>();

        for (var time = _min; time < _max; time += interval)
        {
            ranges.Add((time, time + interval));
        }

        return ranges;
    }
}