using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Domain.Entities.Users;
using Infrastructure.Database;
using Infrastructure.Emails.Contracts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Web.Api.Features.Emails.ViewModels;

namespace Web.Api.Features.BackgroundJobs;

public class SalaryUpdateReminderEmailJob
    : InvocableJobBase<SalaryUpdateReminderEmailJob>
{
    public const int MaximumEmailsPerDay = 80;
    public const int EmailsPerBatch = MaximumEmailsPerDay / 10;

    private static readonly DateTime _startingPoint = new DateTime(
        new DateOnly(2025, 6, 2),
        new TimeOnly(0, 0, 0),
        DateTimeKind.Utc);

    private readonly DatabaseContext _context;
    private readonly ITechinterviewEmailService _emailService;

    public SalaryUpdateReminderEmailJob(
        ILogger<SalaryUpdateReminderEmailJob> logger,
        DatabaseContext context,
        ITechinterviewEmailService emailService)
        : base(logger)
    {
        _context = context;
        _emailService = emailService;
    }

    public override async Task ExecuteAsync(
        CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        if (now < _startingPoint)
        {
            Logger.LogInformation(
                "SalaryUpdateReminderEmailJob is not started yet. Current time: {CurrentTime}, Starting point: {StartingPoint}",
                now,
                _startingPoint);

            return;
        }

        var jobCorrelationId = Guid.NewGuid();

        var todayStart = DateTimeOffset.UtcNow.Date;
        var todayEnd = todayStart.AddDays(1).AddTicks(-1);

        var countOfEmailsSentToday = await _context.UserEmails
            .CountAsync(
                x =>
                    x.CreatedAt >= todayStart &&
                    x.CreatedAt <= todayEnd,
                cancellationToken);

        if (countOfEmailsSentToday >= MaximumEmailsPerDay)
        {
            Logger.LogInformation(
                "Too many salary reminders have been sent today: {CountOfEmailsSentToday}. CorrelationId: {CorrelationId}",
                countOfEmailsSentToday,
                jobCorrelationId);

            return;
        }

        var yearAgo = DateTimeOffset.UtcNow.AddYears(-1);
        var salaryReminderSentEdge = DateTimeOffset.UtcNow.AddMonths(-6);
        var emailSentEdge = DateTimeOffset.UtcNow.AddDays(-7);

        var usersToSend = await _context.Users
            .Include(x => x.Emails.OrderByDescending(u => u.CreatedAt))
            .Include(x => x.Salaries.OrderByDescending(u => u.CreatedAt))
            .Where(x =>
                x.EmailConfirmed &&
                x.UnsubscribeMeFromAll == false &&
                x.DeletedAt == null &&
                x.UniqueToken != null &&
                x.Salaries.Any(s => s.UseInStats))
            .Where(x =>
                x.Salaries.All(y => y.CreatedAt < yearAgo) &&
                (!x.Emails.Any() ||
                 x.Emails
                     .Where(y => y.Type == UserEmailType.SalaryFormReminder)
                     .All(y => y.CreatedAt < salaryReminderSentEdge)))
            .OrderBy(x => x.CreatedAt)
            .Take(EmailsPerBatch)
            .ToListAsync(cancellationToken);

        if (usersToSend.Count == 0)
        {
            Logger.LogInformation(
                "No salary reminders have been sent. No users to be processed. CorrelationId: {CorrelationId}",
                jobCorrelationId);
        }

        var processedCount = 0;
        foreach (var userToSend in usersToSend)
        {
            if (await _emailService.SalaryUpdateReminderEmailAsync(
                    userToSend,
                    cancellationToken))
            {
                processedCount++;

                _context.UserEmails.Add(
                    new UserEmail(
                        SalaryUpdateReminderViewModel.Subject,
                        UserEmailType.SalaryFormReminder,
                        userToSend));

                await Task.Delay(100, cancellationToken);
            }
        }

        if (processedCount > 0)
        {
            await _context.SaveChangesAsync(cancellationToken);
            Logger.LogInformation(
                "Sent {ProcessedCount} salary update reminder emails. Records from db: {RecordsCount}. CorrelationId: {CorrelationId}",
                processedCount,
                usersToSend.Count,
                jobCorrelationId);
        }
        else
        {
            Logger.LogInformation(
                "No salary reminder emails were sent. Records from db: {RecordsCount}. CorrelationId: {CorrelationId}",
                usersToSend.Count,
                jobCorrelationId);
        }
    }
}