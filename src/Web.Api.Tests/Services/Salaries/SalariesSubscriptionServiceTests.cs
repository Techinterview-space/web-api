using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Domain.Entities.Enums;
using Domain.Entities.Salaries;
using Domain.Entities.StatData;
using Domain.Entities.StatData.Salary;
using Microsoft.Extensions.Logging;
using Moq;
using TestUtils.Db;
using TestUtils.Fakes;
using TestUtils.Mocks;
using TestUtils.Services;
using Web.Api.Services.Salaries;
using Xunit;

namespace Web.Api.Tests.Services.Salaries;

public class SalariesSubscriptionServiceTests
{
    [Fact]
    public async Task ProcessAllSubscriptionsAsync_NoSubscriptions_NoData_EmptyResult()
    {
        await using var context = new InMemoryDatabaseContext();

        var target = new SalariesSubscriptionService(
            context,
            new CurrencyServiceFake(),
            new ProfessionsCacheServiceFake(context),
            new GlobalFake(),
            new TelegramBotClientProviderFake(),
            new Mock<ILogger<SalariesSubscriptionService>>().Object);

        var result = await target.ProcessAllSubscriptionsAsync(
            "test",
            default);

        Assert.Equal(0, result);
    }

    [Fact]
    public async Task ProcessAllSubscriptionsAsync_WeeklySubscription_NoPushes_LastMessageWasMonthAgo_Sent()
    {
        await using var context = new InMemoryDatabaseContext();

        var salary1 = await new UserSalaryFake(null, grade: DeveloperGrade.Middle)
            .WithProfession(UserProfessionEnum.BackendDeveloper)
            .PleaseAsync(context);

        var salary2 = await new UserSalaryFake(null, grade: DeveloperGrade.Middle)
            .WithProfession(UserProfessionEnum.FrontendDeveloper)
            .PleaseAsync(context);

        var salary3 = await new UserSalaryFake(null, grade: DeveloperGrade.Middle)
            .WithProfession(UserProfessionEnum.QualityAssurance)
            .PleaseAsync(context);

        var salary4 = await new UserSalaryFake(null, grade: DeveloperGrade.Middle)
            .WithProfession(UserProfessionEnum.Tester)
            .PleaseAsync(context);

        Assert.Equal(4, context.Salaries.Count());

        var lastMessageSent = DateTime.UtcNow.AddDays(-30);
        var subscription = new StatDataChangeSubscriptionFake()
            .WithNoPushesValue(true)
            .WithRegularity(SubscriptionRegularityType.Weekly)
            .WithProfession(
                UserProfessionEnum.BackendDeveloper,
                UserProfessionEnum.FrontendDeveloper,
                UserProfessionEnum.QualityAssurance,
                UserProfessionEnum.Tester)
            .WithLastMessageDate(lastMessageSent)
            .Please(context);

        Assert.Single(subscription.StatDataChangeSubscriptionTgMessages);
        Assert.Equal(lastMessageSent, subscription.StatDataChangeSubscriptionTgMessages[0].CreatedAt);

        var record1 = new StatDataChangeSubscriptionRecordFake(
            subscription,
            null,
            new SalariesStatDataCacheItemSalaryData(
                new List<SalaryBaseData>
                {
                    new SalaryBaseData(salary1),
                    new SalaryBaseData(salary2),
                    new SalaryBaseData(salary3),
                    new SalaryBaseData(salary4),
                },
                4),
            DateTime.UtcNow.AddDays(-7))
            .Please(context);

        var tgProvider = new TelegramBotClientProviderFake();
        var target = new SalariesSubscriptionService(
            context,
            new CurrencyServiceFake(),
            new ProfessionsCacheServiceFake(context),
            new GlobalFake(),
            tgProvider,
            new Mock<ILogger<SalariesSubscriptionService>>().Object);

        context.ChangeTracker.Clear();
        var result = await target.ProcessAllSubscriptionsAsync(
            "test",
            default);

        Assert.Equal(1, result);

        var sentMessages = context.SalariesSubscriptionTelegramMessages.ToList();
        Assert.Equal(2, sentMessages.Count);

        Assert.Equal(subscription.Id, sentMessages[0].SalarySubscriptionId);
        Assert.Equal(subscription.StatDataChangeSubscriptionTgMessages[0].Id, sentMessages[0].Id);

        Assert.Equal(subscription.Id, sentMessages[1].SalarySubscriptionId);
        Assert.NotEqual(subscription.StatDataChangeSubscriptionTgMessages[0].Id, sentMessages[1].Id);
    }

    [Fact]
    public async Task ProcessAllSubscriptionsAsync_WeeklySubscription_NoPushes_LastMessageWasLessThanMonthAgo_NotSent()
    {
        await using var context = new InMemoryDatabaseContext();

        var salary1 = await new UserSalaryFake(null, grade: DeveloperGrade.Middle)
            .WithProfession(UserProfessionEnum.BackendDeveloper)
            .PleaseAsync(context);

        var salary2 = await new UserSalaryFake(null, grade: DeveloperGrade.Middle)
            .WithProfession(UserProfessionEnum.FrontendDeveloper)
            .PleaseAsync(context);

        var salary3 = await new UserSalaryFake(null, grade: DeveloperGrade.Middle)
            .WithProfession(UserProfessionEnum.QualityAssurance)
            .PleaseAsync(context);

        var salary4 = await new UserSalaryFake(null, grade: DeveloperGrade.Middle)
            .WithProfession(UserProfessionEnum.Tester)
            .PleaseAsync(context);

        Assert.Equal(4, context.Salaries.Count());

        var lastMessageSent = DateTime.UtcNow.AddDays(-20);
        var subscription = new StatDataChangeSubscriptionFake()
            .WithNoPushesValue(true)
            .WithRegularity(SubscriptionRegularityType.Weekly)
            .WithProfession(
                UserProfessionEnum.BackendDeveloper,
                UserProfessionEnum.FrontendDeveloper,
                UserProfessionEnum.QualityAssurance,
                UserProfessionEnum.Tester)
            .WithLastMessageDate(lastMessageSent)
            .Please(context);

        Assert.Single(subscription.StatDataChangeSubscriptionTgMessages);
        Assert.Equal(lastMessageSent, subscription.StatDataChangeSubscriptionTgMessages[0].CreatedAt);

        var record1 = new StatDataChangeSubscriptionRecordFake(
            subscription,
            null,
            new SalariesStatDataCacheItemSalaryData(
                new List<SalaryBaseData>
                {
                    new SalaryBaseData(salary1),
                    new SalaryBaseData(salary2),
                    new SalaryBaseData(salary3),
                    new SalaryBaseData(salary4),
                },
                4),
            DateTime.UtcNow.AddDays(-7))
            .Please(context);

        var tgProvider = new TelegramBotClientProviderFake();
        var target = new SalariesSubscriptionService(
            context,
            new CurrencyServiceFake(),
            new ProfessionsCacheServiceFake(context),
            new GlobalFake(),
            tgProvider,
            new Mock<ILogger<SalariesSubscriptionService>>().Object);

        context.ChangeTracker.Clear();
        var result = await target.ProcessAllSubscriptionsAsync(
            "test",
            default);

        Assert.Equal(0, result);

        var sentMessages = context.SalariesSubscriptionTelegramMessages.ToList();
        Assert.Single(sentMessages);

        Assert.Equal(subscription.Id, sentMessages[0].SalarySubscriptionId);
        Assert.Equal(subscription.StatDataChangeSubscriptionTgMessages[0].Id, sentMessages[0].Id);
    }

    [Fact]
    public async Task ProcessAllSubscriptionsAsync_WeeklySubscription_WithPushes_LastMessageWasLessThanMonthAgo_Sent()
    {
        await using var context = new InMemoryDatabaseContext();

        var salary1 = await new UserSalaryFake(null, grade: DeveloperGrade.Middle)
            .WithProfession(UserProfessionEnum.BackendDeveloper)
            .PleaseAsync(context);

        var salary2 = await new UserSalaryFake(null, grade: DeveloperGrade.Middle)
            .WithProfession(UserProfessionEnum.FrontendDeveloper)
            .PleaseAsync(context);

        var salary3 = await new UserSalaryFake(null, grade: DeveloperGrade.Middle)
            .WithProfession(UserProfessionEnum.QualityAssurance)
            .PleaseAsync(context);

        var salary4 = await new UserSalaryFake(null, grade: DeveloperGrade.Middle)
            .WithProfession(UserProfessionEnum.Tester)
            .PleaseAsync(context);

        Assert.Equal(4, context.Salaries.Count());

        var lastMessageSent = DateTime.UtcNow.AddDays(-1);
        var subscription = new StatDataChangeSubscriptionFake()
            .WithNoPushesValue(false)
            .WithRegularity(SubscriptionRegularityType.Weekly)
            .WithProfession(
                UserProfessionEnum.BackendDeveloper,
                UserProfessionEnum.FrontendDeveloper,
                UserProfessionEnum.QualityAssurance,
                UserProfessionEnum.Tester)
            .WithLastMessageDate(lastMessageSent)
            .Please(context);

        Assert.Single(subscription.StatDataChangeSubscriptionTgMessages);
        Assert.Equal(lastMessageSent, subscription.StatDataChangeSubscriptionTgMessages[0].CreatedAt);

        var record1 = new StatDataChangeSubscriptionRecordFake(
            subscription,
            null,
            new SalariesStatDataCacheItemSalaryData(
                new List<SalaryBaseData>
                {
                    new SalaryBaseData(salary1),
                    new SalaryBaseData(salary2),
                    new SalaryBaseData(salary3),
                    new SalaryBaseData(salary4),
                },
                4),
            DateTime.UtcNow.AddDays(-7))
            .Please(context);

        var tgProvider = new TelegramBotClientProviderFake();
        var target = new SalariesSubscriptionService(
            context,
            new CurrencyServiceFake(),
            new ProfessionsCacheServiceFake(context),
            new GlobalFake(),
            tgProvider,
            new Mock<ILogger<SalariesSubscriptionService>>().Object);

        context.ChangeTracker.Clear();
        var result = await target.ProcessAllSubscriptionsAsync(
            "test",
            default);

        Assert.Equal(1, result);

        var sentMessages = context.SalariesSubscriptionTelegramMessages.ToList();
        Assert.Equal(2, sentMessages.Count);

        Assert.Equal(subscription.Id, sentMessages[0].SalarySubscriptionId);
        Assert.Equal(subscription.StatDataChangeSubscriptionTgMessages[0].Id, sentMessages[0].Id);

        Assert.Equal(subscription.Id, sentMessages[1].SalarySubscriptionId);
        Assert.NotEqual(subscription.StatDataChangeSubscriptionTgMessages[0].Id, sentMessages[1].Id);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task ProcessAllSubscriptionsAsync_MonthlySubscription_NoPushes_LastMessageWasLessThanMonthAgo_NotSent(
        bool sentPushes)
    {
        await using var context = new InMemoryDatabaseContext();

        var salary1 = await new UserSalaryFake(null, grade: DeveloperGrade.Middle)
            .WithProfession(UserProfessionEnum.BackendDeveloper)
            .PleaseAsync(context);

        var salary2 = await new UserSalaryFake(null, grade: DeveloperGrade.Middle)
            .WithProfession(UserProfessionEnum.FrontendDeveloper)
            .PleaseAsync(context);

        var salary3 = await new UserSalaryFake(null, grade: DeveloperGrade.Middle)
            .WithProfession(UserProfessionEnum.QualityAssurance)
            .PleaseAsync(context);

        var salary4 = await new UserSalaryFake(null, grade: DeveloperGrade.Middle)
            .WithProfession(UserProfessionEnum.Tester)
            .PleaseAsync(context);

        Assert.Equal(4, context.Salaries.Count());

        var lastMessageSent = DateTime.UtcNow.AddDays(-20);
        var subscription = new StatDataChangeSubscriptionFake()
            .WithNoPushesValue(sentPushes)
            .WithRegularity(SubscriptionRegularityType.Monthly)
            .WithProfession(
                UserProfessionEnum.BackendDeveloper,
                UserProfessionEnum.FrontendDeveloper,
                UserProfessionEnum.QualityAssurance,
                UserProfessionEnum.Tester)
            .WithLastMessageDate(lastMessageSent)
            .Please(context);

        Assert.Single(subscription.StatDataChangeSubscriptionTgMessages);
        Assert.Equal(lastMessageSent, subscription.StatDataChangeSubscriptionTgMessages[0].CreatedAt);

        var record1 = new StatDataChangeSubscriptionRecordFake(
            subscription,
            null,
            new SalariesStatDataCacheItemSalaryData(
                new List<SalaryBaseData>
                {
                    new SalaryBaseData(salary1),
                    new SalaryBaseData(salary2),
                    new SalaryBaseData(salary3),
                    new SalaryBaseData(salary4),
                },
                4),
            DateTime.UtcNow.AddDays(-7))
            .Please(context);

        var tgProvider = new TelegramBotClientProviderFake();
        var target = new SalariesSubscriptionService(
            context,
            new CurrencyServiceFake(),
            new ProfessionsCacheServiceFake(context),
            new GlobalFake(),
            tgProvider,
            new Mock<ILogger<SalariesSubscriptionService>>().Object);

        context.ChangeTracker.Clear();
        var result = await target.ProcessAllSubscriptionsAsync(
            "test",
            default);

        Assert.Equal(0, result);

        var sentMessages = context.SalariesSubscriptionTelegramMessages.ToList();
        Assert.Single(sentMessages);

        Assert.Equal(subscription.Id, sentMessages[0].SalarySubscriptionId);
        Assert.Equal(subscription.StatDataChangeSubscriptionTgMessages[0].Id, sentMessages[0].Id);
    }

    [Fact]
    public async Task ProcessAllSubscriptionsAsync_MonthlySubscription_NoMessagesBefore_Sent()
    {
        await using var context = new InMemoryDatabaseContext();

        var salary1 = await new UserSalaryFake(null, grade: DeveloperGrade.Middle)
            .WithProfession(UserProfessionEnum.BackendDeveloper)
            .PleaseAsync(context);

        var salary2 = await new UserSalaryFake(null, grade: DeveloperGrade.Middle)
            .WithProfession(UserProfessionEnum.FrontendDeveloper)
            .PleaseAsync(context);

        var salary3 = await new UserSalaryFake(null, grade: DeveloperGrade.Middle)
            .WithProfession(UserProfessionEnum.QualityAssurance)
            .PleaseAsync(context);

        var salary4 = await new UserSalaryFake(null, grade: DeveloperGrade.Middle)
            .WithProfession(UserProfessionEnum.Tester)
            .PleaseAsync(context);

        Assert.Equal(4, context.Salaries.Count());

        var subscription = new StatDataChangeSubscriptionFake()
            .WithNoPushesValue(true)
            .WithRegularity(SubscriptionRegularityType.Monthly)
            .WithProfession(
                UserProfessionEnum.BackendDeveloper,
                UserProfessionEnum.FrontendDeveloper,
                UserProfessionEnum.QualityAssurance,
                UserProfessionEnum.Tester)
            .Please(context);

        Assert.Empty(subscription.StatDataChangeSubscriptionTgMessages);

        var tgProvider = new TelegramBotClientProviderFake();
        var target = new SalariesSubscriptionService(
            context,
            new CurrencyServiceFake(),
            new ProfessionsCacheServiceFake(context),
            new GlobalFake(),
            tgProvider,
            new Mock<ILogger<SalariesSubscriptionService>>().Object);

        context.ChangeTracker.Clear();
        var result = await target.ProcessAllSubscriptionsAsync(
            "test",
            default);

        Assert.Equal(1, result);

        var sentMessages = context.SalariesSubscriptionTelegramMessages.ToList();
        Assert.Single(sentMessages);

        Assert.Equal(subscription.Id, sentMessages[0].SalarySubscriptionId);
    }
}