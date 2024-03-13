using System.Collections.Generic;

namespace Domain.Tools;

public static class CollectionExtensions
{
    public static List<T> AddIfDoesNotExist<T>(
        this List<T> list,
        T item)
    {
        if (!list.Contains(item))
        {
            list.Add(item);
        }

        return list;
    }

    public static List<T> AddIfDoesNotExist<T>(
        this List<T> list,
        params T[] items)
    {
        foreach (var item in items)
        {
            list.AddIfDoesNotExist(item);
        }

        return list;
    }
}