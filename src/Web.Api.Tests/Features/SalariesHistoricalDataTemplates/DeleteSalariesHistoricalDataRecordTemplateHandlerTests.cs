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
using Web.Api.Features.SalariesHistoricalDataTemplates.DeleteTemplate;
using Xunit;

namespace Web.Api.Tests.Features.SalariesHistoricalDataTemplates;

public class DeleteSalariesHistoricalDataRecordTemplateHandlerTests
{
    [Fact]
    public async Task Handle_ValidTemplateWithoutRecords_DeletesTemplate()
    {
        // Arrange
        await using var context = new InMemoryDatabaseContext();

        var profession = context.Professions.First(x => x.Id == 1);

        var template = await new SalariesHistoricalDataRecordTemplateFake(
                "Template to Delete",
                new List<long> { profession.Id })
            .PleaseAsync(context);

        context.ChangeTracker.Clear();

        var handler = new DeleteSalariesHistoricalDataRecordTemplateHandler(context);

        var command = new DeleteSalariesHistoricalDataRecordTemplateCommand(template.Id);

        // Act
        await handler.Handle(command, default);

        // Assert
        var deletedTemplate = await context.SalariesHistoricalDataRecordTemplates
            .FirstOrDefaultAsync(x => x.Id == template.Id);

        Assert.Null(deletedTemplate);
    }

    [Fact]
    public async Task Handle_TemplateWithHistoricalRecords_DeletesTemplateAndRecords()
    {
        // Arrange
        await using var context = new InMemoryDatabaseContext();

        var profession = context.Professions.First(x => x.Id == 1);

        var template = await new SalariesHistoricalDataRecordTemplateFake(
                "Template with Records",
                new List<long> { profession.Id })
            .PleaseAsync(context);

        // Create some historical records for this template
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

        await context.SalariesHistoricalDataRecords.AddAsync(record1);
        await context.SalariesHistoricalDataRecords.AddAsync(record2);
        await context.SaveChangesAsync();

        context.ChangeTracker.Clear();

        var handler = new DeleteSalariesHistoricalDataRecordTemplateHandler(context);

        var command = new DeleteSalariesHistoricalDataRecordTemplateCommand(template.Id);

        // Act
        await handler.Handle(command, default);

        // Assert
        var deletedTemplate = await context.SalariesHistoricalDataRecordTemplates
            .FirstOrDefaultAsync(x => x.Id == template.Id);

        Assert.Null(deletedTemplate);

        var deletedRecords = await context.SalariesHistoricalDataRecords
            .Where(x => x.TemplateId == template.Id)
            .ToListAsync();

        Assert.Empty(deletedRecords);
    }

    [Fact]
    public async Task Handle_NonExistentTemplate_ThrowsNotFoundException()
    {
        // Arrange
        await using var context = new InMemoryDatabaseContext();

        var handler = new DeleteSalariesHistoricalDataRecordTemplateHandler(context);

        var nonExistentId = Guid.NewGuid();
        var command = new DeleteSalariesHistoricalDataRecordTemplateCommand(nonExistentId);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<NotFoundException>(
            async () => await handler.Handle(command, default));

        Assert.Contains(nonExistentId.ToString(), exception.Message);
    }

    [Fact]
    public async Task Handle_MultipleTemplatesExist_DeletesOnlySpecifiedTemplate()
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

        var template3 = await new SalariesHistoricalDataRecordTemplateFake(
                "Template 3",
                new List<long> { profession.Id })
            .PleaseAsync(context);

        context.ChangeTracker.Clear();

        var handler = new DeleteSalariesHistoricalDataRecordTemplateHandler(context);

        var command = new DeleteSalariesHistoricalDataRecordTemplateCommand(template2.Id);

        // Act
        await handler.Handle(command, default);

        // Assert
        var remainingTemplates = await context.SalariesHistoricalDataRecordTemplates
            .ToListAsync();

        Assert.Equal(2, remainingTemplates.Count);
        Assert.Contains(remainingTemplates, t => t.Id == template1.Id);
        Assert.Contains(remainingTemplates, t => t.Id == template3.Id);
        Assert.DoesNotContain(remainingTemplates, t => t.Id == template2.Id);
    }

    [Fact]
    public async Task Handle_TemplateWithRecordsFromMultipleTemplates_DeletesOnlyRelatedRecords()
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

        var record1 = new SalariesHistoricalDataRecord(today, template1.Id, salaryData);
        var record2 = new SalariesHistoricalDataRecord(today.AddDays(-1), template1.Id, salaryData);
        var record3 = new SalariesHistoricalDataRecord(today, template2.Id, salaryData);

        await context.SalariesHistoricalDataRecords.AddRangeAsync(record1, record2, record3);
        await context.SaveChangesAsync();

        context.ChangeTracker.Clear();

        var handler = new DeleteSalariesHistoricalDataRecordTemplateHandler(context);

        var command = new DeleteSalariesHistoricalDataRecordTemplateCommand(template1.Id);

        // Act
        await handler.Handle(command, default);

        // Assert
        var remainingRecords = await context.SalariesHistoricalDataRecords
            .ToListAsync();

        Assert.Single(remainingRecords);
        Assert.Equal(template2.Id, remainingRecords[0].TemplateId);
    }
}