using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Domain.Validation.Exceptions;
using Microsoft.EntityFrameworkCore;
using TestUtils.Db;
using TestUtils.Fakes;
using Web.Api.Features.SalariesHistoricalDataTemplates.Shared;
using Web.Api.Features.SalariesHistoricalDataTemplates.UpdateTemplate;
using Xunit;

namespace Web.Api.Tests.Features.SalariesHistoricalDataTemplates;

public class UpdateSalariesHistoricalDataRecordTemplateHandlerTests
{
    [Fact]
    public async Task Handle_ValidRequest_UpdatesTemplate()
    {
        // Arrange
        await using var context = new InMemoryDatabaseContext();

        var profession1 = context.Professions.First(x => x.Id == 1);
        var profession2 = context.Professions.First(x => x.Id == 2);
        var profession3 = context.Professions.First(x => x.Id == 3);

        var existingTemplate = await new SalariesHistoricalDataRecordTemplateFake(
                "Original Name",
                new List<long> { profession1.Id })
            .PleaseAsync(context);

        context.ChangeTracker.Clear();

        var handler = new UpdateSalariesHistoricalDataRecordTemplateHandler(context);

        var command = new UpdateSalariesHistoricalDataRecordTemplateCommand(
            existingTemplate.Id,
            new UpdateSalariesHistoricalDataRecordTemplateBodyRequest
            {
                Name = "Updated Name",
                ProfessionIds = new List<long> { profession2.Id, profession3.Id }
            });

        // Act
        var result = await handler.Handle(command, default);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(existingTemplate.Id, result.Id);
        Assert.Equal("Updated Name", result.Name);
        Assert.Equal(2, result.ProfessionIds.Count);
        Assert.Contains(profession2.Id, result.ProfessionIds);
        Assert.Contains(profession3.Id, result.ProfessionIds);
        Assert.DoesNotContain(profession1.Id, result.ProfessionIds);

        var updatedTemplate = await context.SalariesHistoricalDataRecordTemplates
            .FirstOrDefaultAsync(x => x.Id == existingTemplate.Id);

        Assert.NotNull(updatedTemplate);
        Assert.Equal("Updated Name", updatedTemplate.Name);
        Assert.Equal(2, updatedTemplate.ProfessionIds.Count);
    }

    [Fact]
    public async Task Handle_EmptyName_ThrowsEntityInvalidException()
    {
        // Arrange
        await using var context = new InMemoryDatabaseContext();

        var profession = context.Professions.First(x => x.Id == 1);

        var existingTemplate = await new SalariesHistoricalDataRecordTemplateFake(
                "Original Name",
                new List<long> { profession.Id })
            .PleaseAsync(context);

        context.ChangeTracker.Clear();

        var handler = new UpdateSalariesHistoricalDataRecordTemplateHandler(context);

        var command = new UpdateSalariesHistoricalDataRecordTemplateCommand(
            existingTemplate.Id,
            new UpdateSalariesHistoricalDataRecordTemplateBodyRequest
            {
                Name = string.Empty,
                ProfessionIds = new List<long> { profession.Id }
            });

        // Act & Assert
        var exception = await Assert.ThrowsAsync<Domain.Validation.Exceptions.EntityInvalidException>(
            async () => await handler.Handle(command, default));

        Assert.Contains("Name is invalid", exception.Message);
    }

    [Fact]
    public async Task Handle_NullName_ThrowsEntityInvalidException()
    {
        // Arrange
        await using var context = new InMemoryDatabaseContext();

        var profession = context.Professions.First(x => x.Id == 1);

        var existingTemplate = await new SalariesHistoricalDataRecordTemplateFake(
                "Original Name",
                new List<long> { profession.Id })
            .PleaseAsync(context);

        context.ChangeTracker.Clear();

        var handler = new UpdateSalariesHistoricalDataRecordTemplateHandler(context);

        var command = new UpdateSalariesHistoricalDataRecordTemplateCommand(
            existingTemplate.Id,
            new UpdateSalariesHistoricalDataRecordTemplateBodyRequest
            {
                Name = null,
                ProfessionIds = new List<long> { profession.Id }
            });

        // Act & Assert
        var exception = await Assert.ThrowsAsync<Domain.Validation.Exceptions.EntityInvalidException>(
            async () => await handler.Handle(command, default));

        Assert.Contains("Name is invalid", exception.Message);
    }

    [Fact]
    public async Task Handle_WhitespaceName_ThrowsEntityInvalidException()
    {
        // Arrange
        await using var context = new InMemoryDatabaseContext();

        var profession = context.Professions.First(x => x.Id == 1);

        var existingTemplate = await new SalariesHistoricalDataRecordTemplateFake(
                "Original Name",
                new List<long> { profession.Id })
            .PleaseAsync(context);

        context.ChangeTracker.Clear();

        var handler = new UpdateSalariesHistoricalDataRecordTemplateHandler(context);

        var command = new UpdateSalariesHistoricalDataRecordTemplateCommand(
            existingTemplate.Id,
            new UpdateSalariesHistoricalDataRecordTemplateBodyRequest
            {
                Name = "   ",
                ProfessionIds = new List<long> { profession.Id }
            });

        // Act & Assert
        // Whitespace-only name fails [Required] validation
        var exception = await Assert.ThrowsAsync<Domain.Validation.Exceptions.EntityInvalidException>(
            async () => await handler.Handle(command, default));

        Assert.Contains("Name is invalid", exception.Message);
    }

    [Fact]
    public async Task Handle_NonExistentTemplate_ThrowsNotFoundException()
    {
        // Arrange
        await using var context = new InMemoryDatabaseContext();

        var profession = context.Professions.First(x => x.Id == 1);

        var handler = new UpdateSalariesHistoricalDataRecordTemplateHandler(context);

        var nonExistentId = Guid.NewGuid();
        var command = new UpdateSalariesHistoricalDataRecordTemplateCommand(
            nonExistentId,
            new UpdateSalariesHistoricalDataRecordTemplateBodyRequest
            {
                Name = "Valid Name",
                ProfessionIds = new List<long> { profession.Id }
            });

        // Act & Assert
        var exception = await Assert.ThrowsAsync<NotFoundException>(
            async () => await handler.Handle(command, default));

        Assert.Contains(nonExistentId.ToString(), exception.Message);
    }

    [Fact]
    public async Task Handle_EmptyProfessionIds_ThrowsBadRequestException()
    {
        // Arrange
        await using var context = new InMemoryDatabaseContext();

        var profession = context.Professions.First(x => x.Id == 1);

        var existingTemplate = await new SalariesHistoricalDataRecordTemplateFake(
                "Original Name",
                new List<long> { profession.Id })
            .PleaseAsync(context);

        context.ChangeTracker.Clear();

        var handler = new UpdateSalariesHistoricalDataRecordTemplateHandler(context);

        var command = new UpdateSalariesHistoricalDataRecordTemplateCommand(
            existingTemplate.Id,
            new UpdateSalariesHistoricalDataRecordTemplateBodyRequest
            {
                Name = "Valid Name",
                ProfessionIds = new List<long>()
            });

        // Act & Assert
        var exception = await Assert.ThrowsAsync<BadRequestException>(
            async () => await handler.Handle(command, default));

        Assert.Equal("ProfessionIds is required.", exception.Message);
    }

    [Fact]
    public async Task Handle_InvalidProfessionIds_ThrowsBadRequestException()
    {
        // Arrange
        await using var context = new InMemoryDatabaseContext();

        var profession = context.Professions.First(x => x.Id == 1);

        var existingTemplate = await new SalariesHistoricalDataRecordTemplateFake(
                "Original Name",
                new List<long> { profession.Id })
            .PleaseAsync(context);

        context.ChangeTracker.Clear();

        var handler = new UpdateSalariesHistoricalDataRecordTemplateHandler(context);

        var command = new UpdateSalariesHistoricalDataRecordTemplateCommand(
            existingTemplate.Id,
            new UpdateSalariesHistoricalDataRecordTemplateBodyRequest
            {
                Name = "Valid Name",
                ProfessionIds = new List<long> { 999999 }
            });

        // Act & Assert
        var exception = await Assert.ThrowsAsync<BadRequestException>(
            async () => await handler.Handle(command, default));

        Assert.Equal("Some profession IDs are invalid.", exception.Message);
    }
}