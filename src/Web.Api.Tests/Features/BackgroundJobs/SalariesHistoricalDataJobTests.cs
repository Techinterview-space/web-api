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

public class SalariesHistoricalDataJobTests
{
    [Fact]
    public async Task ExecuteAsync_HappyPath_SalariesExist_DataIsCountedAndSaved()
    {
        // Arrange
        await using var context = new InMemoryDatabaseContext();

        var user = await new UserFake(Role.Interviewer, emailConfirmed: true)
            .PleaseAsync(context);

        var profession = context.Professions.First(x => x.Id == 1);

        var template = await new SalariesHistoricalDataRecordTemplateFake(
                new List<long> { profession.Id })
            .PleaseAsync(context);

        // Create test salaries with grades
        await new UserSalaryFake(
                user,
                value: 1_000_000,
                grade: DeveloperGrade.Junior,
                professionOrNull: profession)
            .PleaseAsync(context);

        await new UserSalaryFake(
                user,
                value: 1500000,
                grade: DeveloperGrade.Middle,
                professionOrNull: profession)
            .PleaseAsync(context);

        await new UserSalaryFake(
                user,
                value: 2000000,
                grade: DeveloperGrade.Senior,
                professionOrNull: profession)
            .PleaseAsync(context);

        context.ChangeTracker.Clear();

        var logger = new Mock<ILogger<SalariesHistoricalDataJob>>();
        var job = new SalariesHistoricalDataJob(logger.Object, context);

        // Act
        await job.ExecuteAsync();

        // Assert
        var records = await context.SalariesHistoricalDataRecords
            .Where(x => x.TemplateId == template.Id)
            .ToListAsync();

        Assert.Single(records);

        var record = records[0];
        var today = new DateTimeOffset(DateTime.UtcNow.Date, TimeSpan.Zero);
        Assert.Equal(today, record.Date);
        Assert.Equal(template.Id, record.TemplateId);
        Assert.NotNull(record.Data);
        Assert.True(record.Data.TotalSalaryCount > 0);
    }

    [Fact]
    public async Task ExecuteAsync_NoSalaries_NoNewRecordIsSaved()
    {
        // Arrange
        await using var context = new InMemoryDatabaseContext();

        var profession = context.Professions.First(x => x.Id == 1);

        var template = await new SalariesHistoricalDataRecordTemplateFake(
                new List<long> { profession.Id })
            .PleaseAsync(context);

        context.ChangeTracker.Clear();

        var logger = new Mock<ILogger<SalariesHistoricalDataJob>>();
        var job = new SalariesHistoricalDataJob(logger.Object, context);

        // Act
        await job.ExecuteAsync();

        // Assert
        var records = await context.SalariesHistoricalDataRecords
            .Where(x => x.TemplateId == template.Id)
            .ToListAsync();

        Assert.Empty(records);

        // Verify that the appropriate log message was written
        logger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("No salaries found")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_ExistingRecordForToday_NoNewRecordIsSaved()
    {
        // Arrange
        await using var context = new InMemoryDatabaseContext();

        var user = await new UserFake(Role.Interviewer, emailConfirmed: true)
            .PleaseAsync(context);

        var profession = context.Professions.First(x => x.Id == 1);

        var template = await new SalariesHistoricalDataRecordTemplateFake(
                new List<long> { profession.Id })
            .PleaseAsync(context);

        // Create test salaries
        await new UserSalaryFake(
                user,
                value: 1_000_000,
                grade: DeveloperGrade.Junior,
                professionOrNull: profession)
            .PleaseAsync(context);

        var today = new DateTimeOffset(DateTime.UtcNow.Date, TimeSpan.Zero);

        // Create an existing record for today
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
            today,
            template.Id,
            existingData);

        await context.SalariesHistoricalDataRecords.AddAsync(existingRecord);
        await context.SaveChangesAsync();

        context.ChangeTracker.Clear();

        var initialRecordsCount = await context.SalariesHistoricalDataRecords
            .CountAsync(x => x.TemplateId == template.Id);

        var logger = new Mock<ILogger<SalariesHistoricalDataJob>>();
        var job = new SalariesHistoricalDataJob(logger.Object, context);

        // Act
        await job.ExecuteAsync();

        // Assert
        var finalRecordsCount = await context.SalariesHistoricalDataRecords
            .CountAsync(x => x.TemplateId == template.Id);

        Assert.Equal(initialRecordsCount, finalRecordsCount);
        Assert.Equal(1, finalRecordsCount);

        // Verify that the appropriate log message was written
        logger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("does exist for today")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }
}
