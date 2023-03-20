using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Domain.Entities;
using Domain.Exceptions;
using Domain.Validation;

namespace Domain.Extensions;

public static class CollectionExtensions
{
    public static T ByIdOrFail<T>(this IEnumerable<T> query, long id)
        where T : class, IHasId
    {
        return query.FirstOrDefault(x => x.Id == id)
               ?? throw ResourceNotFoundException.CreateFromEntity<T>(id);
    }

    public static void AddRange<T>(this ICollection<T> collection, IEnumerable<T> items)
    {
        collection.ThrowIfNull(nameof(collection));
        items.ThrowIfNull(nameof(items));

        foreach (T item in items)
        {
            collection.Add(item);
        }
    }

    public static IReadOnlyCollection<T> CollectionOrEmpty<T>(this IEnumerable<T> items)
    {
        return items?.ToArray() ?? Array.Empty<T>();
    }

    public static IReadOnlyCollection<IReadOnlyCollection<T>> Chunks<T>(this IReadOnlyCollection<T> list, int parts)
    {
        return list
            .Select((item, index) => new { index, item })
            .GroupBy(x => ((x.index + 1) / (list.Count / parts)) + 1)
            .Select(x => x
                .Select(y => y.item)
                .ToArray())
            .ToArray();
    }

    public static async Task<T> FirstItemOrNullAsync<T>(this Task<IReadOnlyCollection<T>> arrayTask)
    {
        return (await arrayTask).FirstOrDefault();
    }

    public static async Task<T> FirstItemOrNullAsync<T>(this Task<T[]> arrayTask)
    {
        return (await arrayTask).FirstOrDefault();
    }

    public static bool NotNullOrEmpty<T>(this IEnumerable<T> items)
    {
        return items != null && items.Any();
    }
}