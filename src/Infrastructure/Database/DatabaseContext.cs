﻿using Domain.Entities;
using Domain.Entities.Companies;
using Domain.Entities.CSV;
using Domain.Entities.Github;
using Domain.Entities.Interviews;
using Domain.Entities.Labels;
using Domain.Entities.Prompts;
using Domain.Entities.Questions;
using Domain.Entities.Salaries;
using Domain.Entities.StatData;
using Domain.Entities.StatData.CompanyReviews;
using Domain.Entities.StatData.Salary;
using Domain.Entities.Telegram;
using Domain.Entities.Users;
using Domain.Validation;
using Domain.Validation.Exceptions;
using Domain.ValueObjects.Dates.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Infrastructure.Database;

public class DatabaseContext : DbContext
{
    public DatabaseContext(DbContextOptions<DatabaseContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users { get; set; }

    public DbSet<InterviewTemplate> InterviewTemplates { get; set; }

    public DbSet<Interview> Interviews { get; set; }

    public DbSet<ShareLink> ShareLinks { get; set; }

    public DbSet<UserLabel> UserLabels { get; set; }

    public DbSet<UserSalary> Salaries { get; set; }

    public DbSet<Skill> Skills { get; set; }

    public DbSet<WorkIndustry> WorkIndustries { get; set; }

    public DbSet<Profession> Professions { get; set; }

    public DbSet<SalariesBotMessage> SalariesBotMessages { get; set; }

    public DbSet<TelegramUserSettings> TelegramUserSettings { get; set; }

    public DbSet<UserCsvDownload> UserCsvDownloads { get; set; }

    public DbSet<SalariesSurveyReply> SalariesSurveyReplies { get; set; }

    public DbSet<StatDataChangeSubscription> SalariesSubscriptions { get; set; }

    public DbSet<StatDataChangeSubscriptionRecord> SalariesSubscriptionRecords { get; set; }

    public DbSet<Company> Companies { get; set; }

    public DbSet<CompanyRatingHistory> CompanyRatingHistoryRecords { get; set; }

    public DbSet<CompanyReview> CompanyReviews { get; set; }

    public DbSet<CompanyReviewVote> CompanyReviewVotes { get; set; }

    public DbSet<AiAnalysisRecord> AiAnalysisSubscriptionRecords { get; set; }

    public DbSet<TelegramInlineReply> TelegramInlineReplies { get; set; }

    public DbSet<SubscriptionTelegramMessage> SalariesSubscriptionTelegramMessages { get; set; }

    public DbSet<UserEmail> UserEmails { get; set; }

    public DbSet<GithubProfile> GithubProfiles { get; set; }

    public DbSet<GithubProfileBotChat> GithubProfileBotChats { get; set; }

    public DbSet<GithubProfileBotMessage> GithubProfileBotMessages { get; set; }

    public DbSet<GithubPersonalUserToken> GithubPersonalUserTokens { get; set; }

    public DbSet<GithubProfileProcessingJob> GithubProfileProcessingJobs { get; set; }

    public DbSet<OpenAiPrompt> OpenAiPrompts { get; set; }

    public DbSet<CompanyOpenAiAnalysis> CompanyOpenAiAnalysisRecords { get; set; }

    public DbSet<LastWeekCompanyReviewsSubscription> CompanyReviewsSubscriptions { get; set; }

    public async Task SaveAsync<TEntity>(
        IReadOnlyCollection<TEntity> entities)
        where TEntity : class, IBaseModel
    {
        entities.ThrowIfNullOrEmpty(nameof(entities));
        foreach (var entity in entities)
        {
            entity.ThrowIfInvalid();
        }

        await AddRangeAsync(entities);
        await TrySaveChangesAsync();
    }

    public virtual async Task<TEntity> SaveAsync<TEntity>(
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

    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        OnBeforeSaving();
        return base.SaveChanges(acceptAllChangesOnSuccess);
    }

    public override Task<int> SaveChangesAsync(
        bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default(CancellationToken))
    {
        OnBeforeSaving();
        return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(typeof(DatabaseContext).Assembly);
    }

    protected virtual void OnBeforeSaving()
    {
        var entries = ChangeTracker.Entries<IHasDates>();
        var currentDateTime = DateTimeOffset.Now;

        foreach (EntityEntry<IHasDates> entry in entries)
        {
            switch (entry.State)
            {
                case EntityState.Modified:
                    entry.Entity.OnUpdate(currentDateTime);
                    break;

                case EntityState.Added:
                    if (!IsInMemory())
                    {
                        entry.Entity.OnCreate(currentDateTime);
                        break;
                    }

                    if (entry.Entity.CreatedAt == default)
                    {
                        entry.Entity.OnCreate(currentDateTime);
                    }

                    break;
            }
        }
    }

    protected virtual bool IsInMemory()
    {
        return false;
    }
}