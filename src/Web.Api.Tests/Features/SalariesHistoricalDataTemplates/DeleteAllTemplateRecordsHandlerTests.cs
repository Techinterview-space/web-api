using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Domain.Entities.HistoricalRecords;
using Domain.Entities.StatData.Salary;
using Domain.Validation.Exceptions;
using Microsoft.EntityFrameworkCore;
using TestUtils.Db;
using TestUtils.Fakes;
using Web.Api.Features.SalariesHistoricalDataTemplates.DeleteAllTemplateRecords;
using Xunit;

namespace Web.Api.Tests.Features.SalariesHistoricalDataTemplates;

public class DeleteAllTemplateRecordsHandlerTests
{
    [Fact]
    public async Task Handle_TemplateWithMultipleRecords_DeletesAllRecordsAndKeepsTemplate()
    {
        // Arrange
        await using var context = new InMemoryDatabaseContext();

        var profession = context.Professions.First(x => x.Id == 1);

        var template = await new SalariesHistoricalDataRecordTemplateFake(
                "Template with Records",
                new List<long> { profession.Id })
            .PleaseAsync(context);

        var today = DateTimeOffset.UtcNow.Date;
        var salaryData = new SalariesStatDataCacheItemSalaryData(
            new List<SalaryBaseData>(),
            0);

        var record1 = new SalariesHistoricalDataRecord(
            today,
            template.Id,
            salaryData);

        var record2 = new SalariesHistoricalDataRecord(
            today.AddDays(-1),
            template.Id,
            salaryData);

        var record3 = new SalariesHistoricalDataRecord(
            today.AddDays(-2),
            template.Id,
            salaryData);

        await context.SalariesHistoricalDataRecords.AddRangeAsync(record1, record2, record3);
        await context.SaveChangesAsync();

        context.ChangeTracker.Clear();

        var handler = new DeleteAllTemplateRecordsHandler(context);

        var command = new DeleteAllTemplateRecordsCommand(template.Id);

        // Act
        await handler.Handle(command, default);

        // Assert
        var templateStillExists = await context.SalariesHistoricalDataRecordTemplates
            .FirstOrDefaultAsync(x => x.Id == template.Id);

        Assert.NotNull(templateStillExists);

        var deletedRecords = await context.SalariesHistoricalDataRecords
            .Where(x => x.TemplateId == template.Id)
            .ToListAsync();

        Assert.Empty(deletedRecords);
    }

    [Fact]
    public async Task Handle_TemplateWithNoRecords_DoesNotThrowError()
    {
        // Arrange
        await using var context = new InMemoryDatabaseContext();

        var profession = context.Professions.First(x => x.Id == 1);

        var template = await new SalariesHistoricalDataRecordTemplateFake(
                "Template without Records",
                new List<long> { profession.Id })
            .PleaseAsync(context);

        context.ChangeTracker.Clear();

        var handler = new DeleteAllTemplateRecordsHandler(context);

        var command = new DeleteAllTemplateRecordsCommand(template.Id);

        // Act
        await handler.Handle(command, default);

        // Assert
        var templateStillExists = await context.SalariesHistoricalDataRecordTemplates
            .FirstOrDefaultAsync(x => x.Id == template.Id);

        Assert.NotNull(templateStillExists);
    }

    [Fact]
    public async Task Handle_MultipleTemplatesWithRecords_DeletesOnlyTargetTemplateRecords()
    {
        // Arrange
        await using var context = new InMemoryDatabaseContext();

        var profession = context.Professions.First(x => x.Id == 1);

        var template1 = await new SalariesHistoricalDataRecordTemplateFake(
                "Template 1",
                new List<long> { profession.Id })
            .PleaseAsync(context);

        var template2 = await new SalariesHistoricalDataRecordTemplateFake(
                "Template 2",
                new List<long> { profession.Id })
            .PleaseAsync(context);

        var today = DateTimeOffset.UtcNow.Date;
        var salaryData = new SalariesStatDataCacheItemSalaryData(
            new List<SalaryBaseData>(),
            0);

        var record1Template1 = new SalariesHistoricalDataRecord(today, template1.Id, salaryData);
        var record2Template1 = new SalariesHistoricalDataRecord(today.AddDays(-1), template1.Id, salaryData);
        var record1Template2 = new SalariesHistoricalDataRecord(today, template2.Id, salaryData);
        var record2Template2 = new SalariesHistoricalDataRecord(today.AddDays(-1), template2.Id, salaryData);

        await context.SalariesHistoricalDataRecords.AddRangeAsync(
            record1Template1,
            record2Template1,
            record1Template2,
            record2Template2);
        await context.SaveChangesAsync();

        context.ChangeTracker.Clear();

        var handler = new DeleteAllTemplateRecordsHandler(context);

        var command = new DeleteAllTemplateRecordsCommand(template1.Id);

        // Act
        await handler.Handle(command, default);

        // Assert
        var template1StillExists = await context.SalariesHistoricalDataRecordTemplates
            .FirstOrDefaultAsync(x => x.Id == template1.Id);

        Assert.NotNull(template1StillExists);

        var template2StillExists = await context.SalariesHistoricalDataRecordTemplates
            .FirstOrDefaultAsync(x => x.Id == template2.Id);

        Assert.NotNull(template2StillExists);

        var template1Records = await context.SalariesHistoricalDataRecords
            .Where(x => x.TemplateId == template1.Id)
            .ToListAsync();

        Assert.Empty(template1Records);

        var template2Records = await context.SalariesHistoricalDataRecords
            .Where(x => x.TemplateId == template2.Id)
            .ToListAsync();

        Assert.Equal(2, template2Records.Count);
    }

    [Fact]
    public async Task Handle_NonExistentTemplate_ThrowsNotFoundException()
    {
        // Arrange
        await using var context = new InMemoryDatabaseContext();

        var handler = new DeleteAllTemplateRecordsHandler(context);

        var nonExistentId = Guid.NewGuid();
        var command = new DeleteAllTemplateRecordsCommand(nonExistentId);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<NotFoundException>(
            async () => await handler.Handle(command, default));

        Assert.Contains(nonExistentId.ToString(), exception.Message);
    }
}