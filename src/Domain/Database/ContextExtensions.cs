using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MG.Utils.Abstract;
using MG.Utils.EFCore;
using MG.Utils.Interfaces;
using MG.Utils.Validation;

namespace Domain.Database;

public static class ContextExtensions
{
    public static async Task SaveAsync<TEntity>(this DatabaseContext context, IReadOnlyCollection<TEntity> entities)
        where TEntity : class, IBaseModel
    {
        entities.ThrowIfNullOrEmpty(nameof(entities));
        foreach (TEntity entity in entities)
        {
            entity.ThrowIfInvalid();
        }

        await context.AddRangeAsync(entities);
        await context.TrySaveChangesAsync();
    }

    public static async Task<long> SaveAsync<TEntity>(this DatabaseContext context, TEntity entity)
        where TEntity : class, IBaseModel
    {
        entity.ThrowIfNull(nameof(entity));
        entity.ThrowIfInvalid();

        var entry = await context.AddAsync(entity);
        await context.TrySaveChangesAsync();

        return entry.Entity.Id;
    }

    public static async Task RemoveAsync<TEntity>(this DatabaseContext context, TEntity entity)
    {
        entity.ThrowIfNull(nameof(entity));
        context.Remove(entity);
        await context.TrySaveChangesAsync();
    }

    public static void RemoveRangeIfNotEmpty<TEntity>(
        this DatabaseContext context,
        IEnumerable<TEntity> entities)
        where TEntity : class
    {
        if (entities.Any())
        {
            context.RemoveRange(entities);
        }
    }
}