using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Domain.Validation.Exceptions;
using Microsoft.EntityFrameworkCore;
using TestUtils.Db;
using TestUtils.Fakes;
using Web.Api.Features.SalariesHistoricalDataTemplates.CreateTemplate;
using Xunit;

namespace Web.Api.Tests.Features.SalariesHistoricalDataTemplates;

public class CreateSalariesHistoricalDataRecordTemplateHandlerTests
{
    [Fact]
    public async Task Handle_ValidRequest_CreatesTemplate()
    {
        // Arrange
        await using var context = new InMemoryDatabaseContext();

        var profession1 = context.Professions.First(x => x.Id == 1);
        var profession2 = context.Professions.First(x => x.Id == 2);

        var handler = new CreateSalariesHistoricalDataRecordTemplateHandler(context);

        var command = new CreateSalariesHistoricalDataRecordTemplateCommand(
            new CreateSalariesHistoricalDataRecordTemplateBodyRequest
            {
                Name = "Backend Developers Template",
                ProfessionIds = new List<long> { profession1.Id, profession2.Id }
            });

        // Act
        var result = await handler.Handle(command, default);

        // Assert
        Assert.NotNull(result);
        Assert.NotEqual(Guid.Empty, result.Id);
        Assert.Equal("Backend Developers Template", result.Name);
        Assert.Equal(2, result.ProfessionIds.Count);
        Assert.Contains(profession1.Id, result.ProfessionIds);
        Assert.Contains(profession2.Id, result.ProfessionIds);

        var savedTemplate = await context.SalariesHistoricalDataRecordTemplates
            .FirstOrDefaultAsync(x => x.Id == result.Id);

        Assert.NotNull(savedTemplate);
        Assert.Equal("Backend Developers Template", savedTemplate.Name);
        Assert.Equal(2, savedTemplate.ProfessionIds.Count);
    }

    [Fact]
    public async Task Handle_EmptyName_ThrowsBadRequestException()
    {
        // Arrange
        await using var context = new InMemoryDatabaseContext();

        var profession = context.Professions.First(x => x.Id == 1);

        var handler = new CreateSalariesHistoricalDataRecordTemplateHandler(context);

        var command = new CreateSalariesHistoricalDataRecordTemplateCommand(
            new CreateSalariesHistoricalDataRecordTemplateBodyRequest
            {
                Name = string.Empty,
                ProfessionIds = new List<long> { profession.Id }
            });

        // Act & Assert
        var exception = await Assert.ThrowsAsync<BadRequestException>(
            async () => await handler.Handle(command, default));

        Assert.Contains("Name cannot be null or empty", exception.Message);
    }

    [Fact]
    public async Task Handle_NullName_ThrowsBadRequestException()
    {
        // Arrange
        await using var context = new InMemoryDatabaseContext();

        var profession = context.Professions.First(x => x.Id == 1);

        var handler = new CreateSalariesHistoricalDataRecordTemplateHandler(context);

        var command = new CreateSalariesHistoricalDataRecordTemplateCommand(
            new CreateSalariesHistoricalDataRecordTemplateBodyRequest
            {
                Name = null,
                ProfessionIds = new List<long> { profession.Id }
            });

        // Act & Assert
        var exception = await Assert.ThrowsAsync<BadRequestException>(
            async () => await handler.Handle(command, default));

        Assert.Contains("Name cannot be null or empty", exception.Message);
    }

    [Fact]
    public async Task Handle_WhitespaceName_ThrowsBadRequestException()
    {
        // Arrange
        await using var context = new InMemoryDatabaseContext();

        var profession = context.Professions.First(x => x.Id == 1);

        var handler = new CreateSalariesHistoricalDataRecordTemplateHandler(context);

        var command = new CreateSalariesHistoricalDataRecordTemplateCommand(
            new CreateSalariesHistoricalDataRecordTemplateBodyRequest
            {
                Name = "   ",
                ProfessionIds = new List<long> { profession.Id }
            });

        // Act & Assert
        var exception = await Assert.ThrowsAsync<BadRequestException>(
            async () => await handler.Handle(command, default));

        Assert.Contains("Name cannot be null or empty", exception.Message);
    }

    [Fact]
    public async Task Handle_EmptyProfessionIds_ThrowsBadRequestException()
    {
        // Arrange
        await using var context = new InMemoryDatabaseContext();

        var handler = new CreateSalariesHistoricalDataRecordTemplateHandler(context);

        var command = new CreateSalariesHistoricalDataRecordTemplateCommand(
            new CreateSalariesHistoricalDataRecordTemplateBodyRequest
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

        var handler = new CreateSalariesHistoricalDataRecordTemplateHandler(context);

        var command = new CreateSalariesHistoricalDataRecordTemplateCommand(
            new CreateSalariesHistoricalDataRecordTemplateBodyRequest
            {
                Name = "Valid Name",
                ProfessionIds = new List<long> { 999999, 888888 }
            });

        // Act & Assert
        var exception = await Assert.ThrowsAsync<BadRequestException>(
            async () => await handler.Handle(command, default));

        Assert.Equal("Some profession IDs are invalid.", exception.Message);
    }

    [Fact]
    public async Task Handle_PartiallyInvalidProfessionIds_ThrowsBadRequestException()
    {
        // Arrange
        await using var context = new InMemoryDatabaseContext();

        var profession = context.Professions.First(x => x.Id == 1);

        var handler = new CreateSalariesHistoricalDataRecordTemplateHandler(context);

        var command = new CreateSalariesHistoricalDataRecordTemplateCommand(
            new CreateSalariesHistoricalDataRecordTemplateBodyRequest
            {
                Name = "Valid Name",
                ProfessionIds = new List<long> { profession.Id, 999999 }
            });

        // Act & Assert
        var exception = await Assert.ThrowsAsync<BadRequestException>(
            async () => await handler.Handle(command, default));

        Assert.Equal("Some profession IDs are invalid.", exception.Message);
    }
}