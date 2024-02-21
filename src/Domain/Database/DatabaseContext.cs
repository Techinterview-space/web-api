using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Domain.Entities;
using Domain.Entities.Employments;
using Domain.Entities.Interviews;
using Domain.Entities.Labels;
using Domain.Entities.Organizations;
using Domain.Entities.Salaries;
using Domain.Entities.Users;
using Domain.Exceptions;
using Domain.Validation;
using Microsoft.EntityFrameworkCore;

namespace Domain.Database;

public class DatabaseContext : AppDbContextBase<DatabaseContext>
{
    public DatabaseContext(DbContextOptions<DatabaseContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users { get; set; }

    public DbSet<InterviewTemplate> InterviewTemplates { get; set; }

    public DbSet<Interview> Interviews { get; set; }

    public DbSet<UserLabel> UserLabels { get; set; }

    public DbSet<CandidateCard> CandidateCards { get; set; }

    public DbSet<Candidate> Candidates { get; set; }

    public DbSet<CandidateInterview> CandidateInterviews { get; set; }

    public DbSet<Organization> Organizations { get; set; }

    public DbSet<JoinToOrgInvitation> JoinToOrgInvitations { get; set; }

    public DbSet<OrganizationUser> OrganizationUsers { get; set; }

    public DbSet<UserSalary> Salaries { get; set; }

    public DbSet<Skill> Skills { get; set; }

    public DbSet<WorkIndustry> WorkIndustries { get; set; }

    public async Task SaveAsync<TEntity>(
        IReadOnlyCollection<TEntity> entities)
        where TEntity : class, IBaseModel
    {
        entities.ThrowIfNullOrEmpty(nameof(entities));
        foreach (TEntity entity in entities)
        {
            entity.ThrowIfInvalid();
        }

        await AddRangeAsync(entities);
        await TrySaveChangesAsync();
    }

    public async Task<TEntity> SaveAsync<TEntity>(
        TEntity entity,
        CancellationToken cancellationToken = default)
        where TEntity : class
    {
        entity.ThrowIfNull(nameof(entity));
        entity.ThrowIfInvalid();

        var entry = await AddAsync(entity, cancellationToken);
        await TrySaveChangesAsync(cancellationToken);

        return entry.Entity;
    }

    public async Task RemoveAsync<TEntity>(
        TEntity entity)
    {
        entity.ThrowIfNull(nameof(entity));
        Remove(entity);
        await TrySaveChangesAsync();
    }

    public void RemoveRangeIfNotEmpty<TEntity>(
        IEnumerable<TEntity> entities)
        where TEntity : class
    {
        if (entities.Any())
        {
            RemoveRange(entities);
        }
    }

    public async Task<int> TrySaveChangesAsync(
        CancellationToken cancellationToken = default)
    {
        try
        {
            return await SaveChangesAsync(cancellationToken);
        }
        catch (Exception exception)
        {
            throw new DatabaseException(exception);
        }
    }

    public async Task<TResult> DoWithinTransactionAsync<TResult>(
        Func<Task<TResult>> action,
        string errorMessage = null,
        CancellationToken cancellationToken = default)
    {
        action.ThrowIfNull(nameof(action));

        try
        {
            await Database.BeginTransactionAsync(cancellationToken);
            var result = await action();
            await Database.CommitTransactionAsync(cancellationToken);
            return result;
        }
        catch (Exception exception)
        {
            await Database.RollbackTransactionAsync(cancellationToken);
            const string defaultError = "Cannot execute transaction due to database error";
            throw new InvalidOperationException(errorMessage ?? defaultError, exception);
        }
    }
}