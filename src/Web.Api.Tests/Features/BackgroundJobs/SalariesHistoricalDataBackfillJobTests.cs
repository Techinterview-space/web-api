using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Domain.Entities.Enums;
using Domain.Entities.HistoricalRecords;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using TestUtils.Db;
using TestUtils.Fakes;
using Web.Api.Features.BackgroundJobs.Salaries;
using Xunit;

namespace Web.Api.Tests.Features.BackgroundJobs;

public class SalariesHistoricalDataBackfillJobTests
{
    [Fact]
    public async Task ExecuteAsync_NoRecordForDate_RecordIsCreated()
    {
        // Arrange
        await using var context = new InMemoryDatabaseContext();

        var user = await new UserFake(Role.Interviewer, emailConfirmed: true)
            .PleaseAsync(context);

        var profession = context.Professions.First(x => x.Id == 1);

        var template = await new SalariesHistoricalDataRecordTemplateFake(
                "Test Template",
                new List<long> { profession.Id })
            .PleaseAsync(context);

        // Create test salaries with grades created 3 days ago
        var threeDaysAgo = DateTimeOffset.UtcNow.AddDays(-3);
        await new UserSalaryFake(
                user,
                value: 1_000_000,
                grade: DeveloperGrade.Junior,
                professionOrNull: profession,
                createdAt: threeDaysAgo)
            .PleaseAsync(context);

        await new UserSalaryFake(
                user,
                value: 1500000,
                grade: DeveloperGrade.Middle,
                professionOrNull: profession,
                createdAt: threeDaysAgo)
            .PleaseAsync(context);

        await new UserSalaryFake(
                user,
                value: 2000000,
                grade: DeveloperGrade.Senior,
                professionOrNull: profession,
                createdAt: threeDaysAgo)
            .PleaseAsync(context);

        context.ChangeTracker.Clear();

        var logger = new Mock<ILogger<SalariesHistoricalDataBackfillJob>>();
        var job = new SalariesHistoricalDataBackfillJob(logger.Object, context);

        // Act
        await job.ExecuteAsync();

        // Assert
        var records = await context.SalariesHistoricalDataRecords
            .Where(x => x.TemplateId == template.Id)
            .OrderBy(x => x.Date)
            .ToListAsync();

        // Should create records for up to 5 days (today, yesterday, 2 days ago, 3 days ago, 4 days ago)
        Assert.True(records.Count > 0);
        Assert.True(records.Count <= 5);

        // Verify at least one record was created
        Assert.All(records, record =>
        {
            Assert.Equal(template.Id, record.TemplateId);
            Assert.NotNull(record.Data);
        });
    }

    [Fact]
    public async Task ExecuteAsync_RecordAlreadyExistsForDate_NoNewRecordCreated()
    {
        // Arrange
        await using var context = new InMemoryDatabaseContext();

        var user = await new UserFake(Role.Interviewer, emailConfirmed: true)
            .PleaseAsync(context);

        var profession = context.Professions.First(x => x.Id == 1);

        var template = await new SalariesHistoricalDataRecordTemplateFake(
                "Test Template",
                new List<long> { profession.Id })
            .PleaseAsync(context);

        // Create test salaries
        await new UserSalaryFake(
                user,
                value: 1_000_000,
                grade: DeveloperGrade.Junior,
                professionOrNull: profession)
            .PleaseAsync(context);

        var yesterday = new DateTimeOffset(DateTime.UtcNow.Date.AddDays(-1), TimeSpan.Zero);

        // Create an existing record for yesterday
        var salaries = await context.Salaries
            .Where(x => x.Grade.HasValue && x.ProfessionId == profession.Id)
            .Select(x => new Domain.Entities.StatData.Salary.SalaryBaseData
            {
                ProfessionId = x.ProfessionId,
                Grade = x.Grade.Value,
                Value = x.Value,
                CreatedAt = x.CreatedAt,
            })
            .ToListAsync();

        var existingData = new Domain.Entities.StatData.Salary.SalariesStatDataCacheItemSalaryData(
            salaries,
            salaries.Count);

        var existingRecord = new SalariesHistoricalDataRecord(
            yesterday,
            template.Id,
            existingData);

        await context.SalariesHistoricalDataRecords.AddAsync(existingRecord);
        await context.SaveChangesAsync();

        context.ChangeTracker.Clear();

        var initialRecordsCountForYesterday = await context.SalariesHistoricalDataRecords
            .CountAsync(x => x.TemplateId == template.Id && x.Date == yesterday);

        Assert.Equal(1, initialRecordsCountForYesterday);

        var logger = new Mock<ILogger<SalariesHistoricalDataBackfillJob>>();
        var job = new SalariesHistoricalDataBackfillJob(logger.Object, context);

        // Act
        await job.ExecuteAsync();

        // Assert
        var finalRecordsCountForYesterday = await context.SalariesHistoricalDataRecords
            .CountAsync(x => x.TemplateId == template.Id && x.Date == yesterday);

        // Should still be 1 - no duplicate created
        Assert.Equal(1, finalRecordsCountForYesterday);
    }

    [Fact]
    public async Task ExecuteAsync_DateEarlierThan20250101_NotProcessed()
    {
        // Arrange
        await using var context = new InMemoryDatabaseContext();

        var user = await new UserFake(Role.Interviewer, emailConfirmed: true)
            .PleaseAsync(context);

        var profession = context.Professions.First(x => x.Id == 1);

        var template = await new SalariesHistoricalDataRecordTemplateFake(
                "Test Template",
                new List<long> { profession.Id })
            .PleaseAsync(context);

        // Create test salaries created before 2025-01-01
        var earlyDate = new DateTimeOffset(new DateTime(2024, 12, 15), TimeSpan.Zero);
        await new UserSalaryFake(
                user,
                value: 1_000_000,
                grade: DeveloperGrade.Junior,
                professionOrNull: profession,
                createdAt: earlyDate)
            .PleaseAsync(context);

        context.ChangeTracker.Clear();

        var logger = new Mock<ILogger<SalariesHistoricalDataBackfillJob>>();
        var job = new SalariesHistoricalDataBackfillJob(logger.Object, context);

        // Act
        await job.ExecuteAsync();

        // Assert
        var earliestAllowedDate = new DateTimeOffset(new DateTime(2025, 1, 1), TimeSpan.Zero);
        var recordsBeforeEarliestDate = await context.SalariesHistoricalDataRecords
            .Where(x => x.TemplateId == template.Id && x.Date < earliestAllowedDate)
            .ToListAsync();

        // No records should be created for dates before 2025-01-01
        Assert.Empty(recordsBeforeEarliestDate);
    }

    [Fact]
    public async Task ExecuteAsync_ProcessesMaximumFiveDays()
    {
        // Arrange
        await using var context = new InMemoryDatabaseContext();

        var user = await new UserFake(Role.Interviewer, emailConfirmed: true)
            .PleaseAsync(context);

        var profession = context.Professions.First(x => x.Id == 1);

        var template = await new SalariesHistoricalDataRecordTemplateFake(
                "Test Template",
                new List<long> { profession.Id })
            .PleaseAsync(context);

        // Create salaries with grades created 10 days ago (to have enough data for multiple days)
        var tenDaysAgo = DateTimeOffset.UtcNow.AddDays(-10);
        for (int i = 0; i < 3; i++)
        {
            await new UserSalaryFake(
                    user,
                    value: 1_000_000 + (i * 500000),
                    grade: DeveloperGrade.Junior,
                    professionOrNull: profession,
                    createdAt: tenDaysAgo)
                .PleaseAsync(context);
        }

        context.ChangeTracker.Clear();

        var logger = new Mock<ILogger<SalariesHistoricalDataBackfillJob>>();
        var job = new SalariesHistoricalDataBackfillJob(logger.Object, context);

        // Act
        await job.ExecuteAsync();

        // Assert
        var records = await context.SalariesHistoricalDataRecords
            .Where(x => x.TemplateId == template.Id)
            .ToListAsync();

        // Should process at most 5 days
        Assert.True(records.Count <= 5);
    }

    [Fact]
    public async Task ExecuteAsync_NoTemplates_NoProcessing()
    {
        // Arrange
        await using var context = new InMemoryDatabaseContext();

        var logger = new Mock<ILogger<SalariesHistoricalDataBackfillJob>>();
        var job = new SalariesHistoricalDataBackfillJob(logger.Object, context);

        // Act
        await job.ExecuteAsync();

        // Assert
        var records = await context.SalariesHistoricalDataRecords.ToListAsync();
        Assert.Empty(records);

        // Verify that the appropriate log message was written
        logger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("No templates to be processed")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }
}
