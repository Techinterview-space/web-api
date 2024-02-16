using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Domain.Entities;
using Domain.Exceptions;
using Domain.Validation;
using Domain.ValueObjects.Pagination;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;

namespace Domain.Database;

public static class ContextExtensions
{
    public static IQueryable<TEntity> Active<TEntity>(this IQueryable<TEntity> set)
        where TEntity : class, IHasDeletedAt
    {
        return set.Where(x => x.DeletedAt == null);
    }

    public static async Task<TEntity> AddEntityAsync<TContext, TEntity>(
        this TContext set,
        TEntity entity,
        CancellationToken cancellationToken = default)
        where TContext : DbContext
        where TEntity : class
    {
        var entry = await set.AddAsync(entity, cancellationToken);

        return entry.Entity;
    }

    public static async Task AddRangeAsync<TContext, TEntity>(
        this TContext set,
        IEnumerable<TEntity> entity,
        CancellationToken cancellationToken = default)
        where TContext : DbContext
        where TEntity : class
    {
        await set.AddRangeAsync(entity, cancellationToken);
    }

    /// <summary>
    /// Returns an array with no tracking entities.
    /// </summary>
    /// <typeparam name="T">Type.</typeparam>
    /// <param name="query">Query.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Array.</returns>
    public static Task<T[]> AllAsync<T>(
        this IQueryable<T> query,
        CancellationToken cancellationToken = default)
        where T : class
    {
        return query
            .AsNoTracking()
            .ToArrayAsync(cancellationToken);
    }

    public static async Task<IEnumerable<TResultEntity>> AllAsync<TEntity, TResultEntity>(
        this IQueryable<TEntity> query,
        Func<TEntity, TResultEntity> transform,
        CancellationToken cancellationToken = default)
        where TEntity : class
        where TResultEntity : class
    {
        return (await query
                .AllAsync(cancellationToken))
            .Select(transform);
    }

    public static Task<T> ByIdOrNullAsync<T>(
        this IQueryable<T> query,
        long id,
        CancellationToken cancellationToken = default)
        where T : class, IHasId =>
        query.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    public static Task<T> ByIdOrNullAsync<T>(
        this IQueryable<T> query,
        Guid id,
        CancellationToken cancellationToken = default)
        where T : class, IHasIdBase<Guid> =>
        query.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    public static async Task HasEntityOrFailAsync<T>(
        this IQueryable<T> query,
        long id,
        CancellationToken cancellationToken = default)
        where T : class, IHasId
    {
        if (!await query.AnyAsync(x => x.Id == id, cancellationToken))
        {
            throw ResourceNotFoundException.CreateFromEntity<T>(id);
        }
    }

    public static async Task HasEntitiesOrFailAsync<T>(
        this IQueryable<T> query,
        IReadOnlyCollection<long> ids,
        CancellationToken cancellationToken = default)
        where T : class, IHasId
    {
        if (ids.Except(await query.Select(x => x.Id).ToArrayAsync(cancellationToken)).Any())
        {
            throw new BadRequestException($"Incorrect {typeof(T).Name} ids");
        }
    }

    public static async Task<IReadOnlyCollection<TDto>> MapAsync<TEntity, TDto>(
        this IQueryable<TEntity> query, Func<TEntity, TDto> converter, CancellationToken cancellationToken = default(CancellationToken))
        where TEntity : class
    {
        return (await query.AllAsync(cancellationToken))
            .Select(converter)
            .ToArray();
    }

    public static async Task<T> ByIdOrFailAsync<T>(this IQueryable<T> query, long id, CancellationToken cancellationToken = default(CancellationToken))
        where T : class, IHasId
    {
        return await query.FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
               ?? throw ResourceNotFoundException.CreateFromEntity<T>(id);
    }

    public static async Task<T> ByIdOrFailAsync<T>(this IQueryable<T> query, Guid id, CancellationToken cancellationToken = default(CancellationToken))
        where T : class, IHasIdBase<Guid>
    {
        return await query.FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
               ?? throw ResourceNotFoundException.CreateFromEntity<T>(id);
    }

    public static async Task<Pageable<TEntity>> AsPaginatedAsync<TEntity>(
        this IQueryable<TEntity> query,
        PageModel pageModelOrNull = null,
        CancellationToken cancellationToken = default(CancellationToken))
        where TEntity : class
    {
        pageModelOrNull ??= PageModel.Default;

        return new Pageable<TEntity>(
            currentPage: pageModelOrNull.Page,
            pageSize: pageModelOrNull.PageSize,
            totalItems: await query.CountAsync(cancellationToken),
            results: await query
                .Skip(pageModelOrNull.ToSkip)
                .Take(pageModelOrNull.PageSize)
                .AllAsync(cancellationToken));
    }

    public static IQueryable<TEntity> AsPaginated<TEntity>(
        this IQueryable<TEntity> query,
        PageModel pageModelOrNull = null)
    {
        pageModelOrNull ??= PageModel.Default;

        return query
            .Skip(pageModelOrNull.ToSkip)
            .Take(pageModelOrNull.PageSize);
    }

    public static async Task<Pageable<TResultEntity>> AsPaginatedAsync<TEntity, TResultEntity>(
        this IQueryable<TEntity> query,
        Func<TEntity, TResultEntity> transform,
        PageModel pageModelOrNull = null,
        CancellationToken cancellationToken = default(CancellationToken))
        where TEntity : class
        where TResultEntity : class
    {
        pageModelOrNull ??= PageModel.Default;

        return new Pageable<TResultEntity>(
            currentPage: pageModelOrNull.Page,
            pageSize: pageModelOrNull.PageSize,
            totalItems: await query.CountAsync(cancellationToken),
            results: (await query
                .Skip(pageModelOrNull.ToSkip)
                .Take(pageModelOrNull.PageSize)
                .AsNoTracking()
                .ToArrayAsync(cancellationToken))
            .Select(transform)
            .ToArray());
    }

    public static IQueryable<T> When<T>(
        this IQueryable<T> query,
        bool condition,
        Expression<Func<T, bool>> whereExpression)
        where T : class
    {
        return condition ? query.Where(whereExpression) : query;
    }

    public static async Task AnyOrFailAsync<T>(
        this IQueryable<T> context,
        Expression<Func<T, bool>> expression,
        string errorMessage = null,
        CancellationToken cancellationToken = default(CancellationToken))
        where T : class
    {
        if (!await context.AnyAsync(expression, cancellationToken))
        {
            throw new ResourceNotFoundException(errorMessage ?? "There is no item found by a passed condition");
        }
    }

    public static async Task NoItemsByConditionOrFailAsync<T>(
        this IQueryable<T> context,
        Expression<Func<T, bool>> expression,
        string errorMessage = null,
        CancellationToken cancellationToken = default(CancellationToken))
        where T : class
    {
        if (await context.AnyAsync(expression, cancellationToken))
        {
            throw new BadRequestException(errorMessage ?? "There are items found by a passed condition");
        }
    }

    public static async Task NoItemsByConditionOrFailAsync<T>(
        this IQueryable<T> context,
        Expression<Func<T, bool>> expression,
        Exception exception,
        CancellationToken cancellationToken = default(CancellationToken))
        where T : class
    {
        if (await context.AnyAsync(expression, cancellationToken))
        {
            throw exception;
        }
    }

    public static IQueryable<TEntity> IncludeWhen<TEntity, TProperty>(
        this IQueryable<TEntity> query,
        bool condition,
        Expression<Func<TEntity, TProperty>> navigationPropertyPath)
        where TEntity : class
    {
        return condition ? query.Include(navigationPropertyPath) : query;
    }

    public static IQueryable<TEntity> IncludeWhen<TEntity, TProperty>(
        this IQueryable<TEntity> query,
        bool condition,
        Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, TProperty>> include)
        where TEntity : class
    {
        return condition ? include(query) : query;
    }

    public static IQueryable<long> GetIds<TEntity>(
        this IQueryable<TEntity> query)
        where TEntity : BaseModel
    {
        return query.Select(x => x.Id);
    }

    public static void RemoveRangeIfAny<TEntity>(
        this DbSet<TEntity> query,
        IEnumerable<TEntity> entitiesToRemove)
        where TEntity : class
    {
        var toRemove = entitiesToRemove as IReadOnlyCollection<TEntity> ?? entitiesToRemove.ToArray();
        if (toRemove.Any())
        {
            query.RemoveRange(toRemove);
        }
    }
}