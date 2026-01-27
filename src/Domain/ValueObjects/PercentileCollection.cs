using System;
using System.Collections.Generic;
using System.Linq;

namespace Domain.ValueObjects;

public class PercentileCollection<T>
{
    private readonly int _minPercentile;
    private readonly int _maxPercentile;
    private readonly IEnumerable<T> _source;

    public PercentileCollection(
        int minPercentile,
        int maxPercentile,
        IEnumerable<T> source)
    {
        _minPercentile = minPercentile;
        _maxPercentile = maxPercentile;
        _source = source;
    }

    public List<T> ToList()
    {
        var result = new List<T>();
        var sourceList = _source.ToList();
        var count = sourceList.Count;
        var minIndex = (int)Math.Floor(count * _minPercentile / 100.0);
        var maxIndex = (int)Math.Floor(count * _maxPercentile / 100.0);

        for (var i = minIndex; i < maxIndex; i++)
        {
            result.Add(sourceList[i]);
        }

        return result;
    }
}