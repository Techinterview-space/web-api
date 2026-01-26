using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Domain.Entities.Users;
using Domain.Enums;
using Infrastructure.Database;
using Infrastructure.Emails.Contracts;
using Microsoft.Extensions.Logging;
using Moq;
using TestUtils.Db;
using TestUtils.Fakes;
using Web.Api.Features.BackgroundJobs;
using Web.Api.Features.BackgroundJobs.Salaries;
using Xunit;

namespace Web.Api.Tests.Features.BackgroundJobs;

public class SalaryUpdateReminderEmailJobTests
{
    [Fact]
    public async Task Handle_UserHasOldSalary_NoEmails_Sent()
    {
        await using var context = new InMemoryDatabaseContext();

        var user1 = await new UserFake(Role.Interviewer, emailConfirmed: true)
            .WithUnsubscribeMeFromAll(false)
            .PleaseAsync(context);

        var salary1 = await new UserSalaryFake(
            user1,
            createdAt: DateTimeOffset.UtcNow.AddYears(-1).AddDays(-1))
            .PleaseAsync(context);

        var salary2 = await new UserSalaryFake(
                user1,
                createdAt: DateTimeOffset.UtcNow.AddYears(-1).AddDays(-10))
            .PleaseAsync(context);

        var emailService = new Mock<ITechinterviewEmailService>();
        emailService.Setup(x => x.SalaryUpdateReminderEmailAsync(
                It.Is<User>(u => u.Id == user1.Id),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        context.ChangeTracker.Clear();
        Assert.Equal(0, context.UserEmails.Count());

        var target = new SalaryUpdateReminderEmailJobTestable(
            new Mock<ILogger<SalaryUpdateReminderEmailJob>>().Object,
            context,
            emailService.Object);

        await target.ExecuteAsync();

        var allEmails = context.UserEmails.ToList();
        Assert.Single(allEmails);

        Assert.Equal(user1.Id, allEmails[0].UserId);

        emailService.Verify(
            x => x.SalaryUpdateReminderEmailAsync(
                It.Is<User>(u => u.Id == user1.Id),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_UserHasOldSalary_NoEmails_UnsubscribeMeFromAllEmails_NotSent()
    {
        await using var context = new InMemoryDatabaseContext();

        var user1 = await new UserFake(Role.Interviewer, emailConfirmed: true)
            .WithUnsubscribeMeFromAll(true)
            .PleaseAsync(context);

        var salary1 = await new UserSalaryFake(
                user1,
                createdAt: DateTimeOffset.UtcNow.AddYears(-1).AddDays(-1))
            .PleaseAsync(context);

        var salary2 = await new UserSalaryFake(
                user1,
                createdAt: DateTimeOffset.UtcNow.AddYears(-1).AddDays(-10))
            .PleaseAsync(context);

        var emailService = new Mock<ITechinterviewEmailService>();
        emailService.Setup(x => x.SalaryUpdateReminderEmailAsync(
                It.Is<User>(u => u.Id == user1.Id),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        context.ChangeTracker.Clear();
        Assert.Equal(0, context.UserEmails.Count());

        var target = new SalaryUpdateReminderEmailJobTestable(
            new Mock<ILogger<SalaryUpdateReminderEmailJob>>().Object,
            context,
            emailService.Object);

        await target.ExecuteAsync();

        var allEmails = context.UserEmails.ToList();
        Assert.Empty(allEmails);

        emailService.Verify(
            x => x.SalaryUpdateReminderEmailAsync(
                It.Is<User>(u => u.Id == user1.Id),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_UserHasOldSalary_SalaryFormReminder_EmailWasSentInThPastEnough_Sent()
    {
        await using var context = new InMemoryDatabaseContext();

        var user1 = await new UserFake(Role.Interviewer, emailConfirmed: true)
            .WithUnsubscribeMeFromAll(false)
            .WithUserEmail(
                UserEmailType.SalaryFormReminder,
                createdAt: DateTimeOffset.UtcNow.AddYears(-1).AddDays(-1))
            .PleaseAsync(context);

        var salary1 = await new UserSalaryFake(
                user1,
                createdAt: DateTimeOffset.UtcNow.AddYears(-1).AddDays(-1))
            .PleaseAsync(context);

        var salary2 = await new UserSalaryFake(
                user1,
                createdAt: DateTimeOffset.UtcNow.AddYears(-1).AddDays(-10))
            .PleaseAsync(context);

        var emailService = new Mock<ITechinterviewEmailService>();
        emailService.Setup(x => x.SalaryUpdateReminderEmailAsync(
                It.Is<User>(u => u.Id == user1.Id),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        context.ChangeTracker.Clear();
        Assert.Equal(1, context.UserEmails.Count());

        var target = new SalaryUpdateReminderEmailJobTestable(
            new Mock<ILogger<SalaryUpdateReminderEmailJob>>().Object,
            context,
            emailService.Object);

        await target.ExecuteAsync();

        var allEmails = context.UserEmails.ToList();
        Assert.Equal(2, allEmails.Count);

        Assert.Equal(user1.Id, allEmails[1].UserId);

        emailService.Verify(
            x => x.SalaryUpdateReminderEmailAsync(
                It.Is<User>(u => u.Id == user1.Id),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Theory]
    [InlineData(UserEmailType.CompanyReviewNotification)]
    public async Task Handle_UserHasOldSalary_EmailWasSentInThPastEnough_OtherEmail_Sent(
        UserEmailType emailInHistory)
    {
        await using var context = new InMemoryDatabaseContext();

        var user1 = await new UserFake(Role.Interviewer, emailConfirmed: true)
            .WithUnsubscribeMeFromAll(false)
            .WithUserEmail(
                emailInHistory,
                createdAt: DateTimeOffset.UtcNow.AddDays(-8))
            .PleaseAsync(context);

        var salary1 = await new UserSalaryFake(
                user1,
                createdAt: DateTimeOffset.UtcNow.AddYears(-1).AddDays(-1))
            .PleaseAsync(context);

        var salary2 = await new UserSalaryFake(
                user1,
                createdAt: DateTimeOffset.UtcNow.AddYears(-1).AddDays(-10))
            .PleaseAsync(context);

        var emailService = new Mock<ITechinterviewEmailService>();
        emailService.Setup(x => x.SalaryUpdateReminderEmailAsync(
                It.Is<User>(u => u.Id == user1.Id),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        context.ChangeTracker.Clear();
        Assert.Equal(1, context.UserEmails.Count());

        var target = new SalaryUpdateReminderEmailJobTestable(
            new Mock<ILogger<SalaryUpdateReminderEmailJob>>().Object,
            context,
            emailService.Object);

        await target.ExecuteAsync();

        var allEmails = context.UserEmails.ToList();
        Assert.Equal(2, allEmails.Count);

        Assert.Equal(user1.Id, allEmails[1].UserId);

        emailService.Verify(
            x => x.SalaryUpdateReminderEmailAsync(
                It.Is<User>(u => u.Id == user1.Id),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_UserHasOldSalary_SalaryFormReminder_EmailWasSentRecently_NotSent()
    {
        await using var context = new InMemoryDatabaseContext();

        var user1 = await new UserFake(Role.Interviewer, emailConfirmed: true)
            .WithUnsubscribeMeFromAll(false)
            .WithUserEmail(
                UserEmailType.SalaryFormReminder,
                createdAt: DateTimeOffset.UtcNow.AddMonths(-3).AddDays(1))
            .PleaseAsync(context);

        var salary1 = await new UserSalaryFake(
                user1,
                createdAt: DateTimeOffset.UtcNow.AddYears(-1).AddDays(-1))
            .PleaseAsync(context);

        var salary2 = await new UserSalaryFake(
                user1,
                createdAt: DateTimeOffset.UtcNow.AddYears(-1).AddDays(-10))
            .PleaseAsync(context);

        var emailService = new Mock<ITechinterviewEmailService>();
        emailService.Setup(x => x.SalaryUpdateReminderEmailAsync(
                It.Is<User>(u => u.Id == user1.Id),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        context.ChangeTracker.Clear();
        Assert.Equal(1, context.UserEmails.Count());

        var target = new SalaryUpdateReminderEmailJobTestable(
            new Mock<ILogger<SalaryUpdateReminderEmailJob>>().Object,
            context,
            emailService.Object);

        await target.ExecuteAsync();

        var allEmails = context.UserEmails.ToList();
        Assert.Single(allEmails);

        emailService.Verify(
            x => x.SalaryUpdateReminderEmailAsync(
                It.Is<User>(u => u.Id == user1.Id),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_ManyEmailsWereSentToday_Sent()
    {
        await using var context = new InMemoryDatabaseContext();

        var user1 = await new UserFake(Role.Interviewer, emailConfirmed: true)
            .WithUnsubscribeMeFromAll(false)
            .PleaseAsync(context);

        var user2 = await new UserFake(Role.Interviewer, emailConfirmed: true)
            .WithUnsubscribeMeFromAll(false)
            .PleaseAsync(context);

        for (var i = 0; i < SalaryUpdateReminderEmailJob.MaximumEmailsPerDay; i++)
        {
            context.Add(
                new UserEmail(
                    "Hello!",
                    UserEmailType.SalaryFormReminder,
                    user2));
        }

        await context.SaveChangesAsync();

        var salary1 = await new UserSalaryFake(
                user1,
                createdAt: DateTimeOffset.UtcNow.AddYears(-1).AddDays(-1))
            .PleaseAsync(context);

        var emailService = new Mock<ITechinterviewEmailService>();
        emailService.Setup(x => x.SalaryUpdateReminderEmailAsync(
                It.Is<User>(u => u.Id == user1.Id),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        context.ChangeTracker.Clear();
        Assert.Equal(SalaryUpdateReminderEmailJob.MaximumEmailsPerDay, context.UserEmails.Count());

        var target = new SalaryUpdateReminderEmailJobTestable(
            new Mock<ILogger<SalaryUpdateReminderEmailJob>>().Object,
            context,
            emailService.Object);

        await target.ExecuteAsync();

        Assert.Equal(SalaryUpdateReminderEmailJob.MaximumEmailsPerDay, context.UserEmails.Count());

        emailService.Verify(
            x => x.SalaryUpdateReminderEmailAsync(
                It.Is<User>(u => u.Id == user1.Id),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    private class SalaryUpdateReminderEmailJobTestable : SalaryUpdateReminderEmailJob
    {
        public SalaryUpdateReminderEmailJobTestable(
            ILogger<SalaryUpdateReminderEmailJob> logger,
            DatabaseContext context,
            ITechinterviewEmailService emailService)
            : base(logger, context, emailService)
        {
        }

        protected override Task ShortDelayAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}