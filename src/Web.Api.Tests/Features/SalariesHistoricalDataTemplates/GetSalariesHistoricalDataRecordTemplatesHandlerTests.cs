using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TestUtils.Db;
using TestUtils.Fakes;
using Web.Api.Features.SalariesHistoricalDataTemplates.GetTemplates;
using Xunit;

namespace Web.Api.Tests.Features.SalariesHistoricalDataTemplates;

public class GetSalariesHistoricalDataRecordTemplatesHandlerTests
{
    [Fact]
    public async Task Handle_MultipleTemplatesExist_ReturnsAllOrderedByCreatedAtDescending()
    {
        // Arrange
        await using var context = new InMemoryDatabaseContext();

        var profession1 = context.Professions.First(x => x.Id == 1);
        var profession2 = context.Professions.First(x => x.Id == 2);

        var template1 = await new SalariesHistoricalDataRecordTemplateFake(
                "Template A",
                new List<long> { profession1.Id })
            .PleaseAsync(context);

        await Task.Delay(10); // Ensure different CreatedAt

        var template2 = await new SalariesHistoricalDataRecordTemplateFake(
                "Template B",
                new List<long> { profession2.Id })
            .PleaseAsync(context);

        await Task.Delay(10);

        var template3 = await new SalariesHistoricalDataRecordTemplateFake(
                "Template C",
                new List<long> { profession1.Id, profession2.Id })
            .PleaseAsync(context);

        context.ChangeTracker.Clear();

        var handler = new GetSalariesHistoricalDataRecordTemplatesHandler(context);

        var query = new GetSalariesHistoricalDataRecordTemplatesQuery
        {
            Page = 1,
            PageSize = 10
        };

        // Act
        var result = await handler.Handle(query, default);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(3, result.TotalItems);
        Assert.Equal(3, result.Results.Count);

        var resultsList = result.Results.ToList();

        // Verify ordering - most recent first
        Assert.Equal(template3.Id, resultsList[0].Id);
        Assert.Equal("Template C", resultsList[0].Name);

        Assert.Equal(template2.Id, resultsList[1].Id);
        Assert.Equal("Template B", resultsList[1].Name);

        Assert.Equal(template1.Id, resultsList[2].Id);
        Assert.Equal("Template A", resultsList[2].Name);

        // Verify all templates have Name property populated
        Assert.All(result.Results, template => Assert.NotNull(template.Name));
    }

    [Fact]
    public async Task Handle_NoTemplatesExist_ReturnsEmptyList()
    {
        // Arrange
        await using var context = new InMemoryDatabaseContext();

        var handler = new GetSalariesHistoricalDataRecordTemplatesHandler(context);

        var query = new GetSalariesHistoricalDataRecordTemplatesQuery
        {
            Page = 1,
            PageSize = 10
        };

        // Act
        var result = await handler.Handle(query, default);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(0, result.TotalItems);
        Assert.Empty(result.Results);
    }

    [Fact]
    public async Task Handle_PaginationWorks_ReturnsCorrectPage()
    {
        // Arrange
        await using var context = new InMemoryDatabaseContext();

        var profession = context.Professions.First(x => x.Id == 1);

        // Create 5 templates
        for (var i = 1; i <= 5; i++)
        {
            await new SalariesHistoricalDataRecordTemplateFake(
                    $"Template {i}",
                    new List<long> { profession.Id })
                .PleaseAsync(context);

            await Task.Delay(10);
        }

        context.ChangeTracker.Clear();

        var handler = new GetSalariesHistoricalDataRecordTemplatesHandler(context);

        var query = new GetSalariesHistoricalDataRecordTemplatesQuery
        {
            Page = 2,
            PageSize = 2
        };

        // Act
        var result = await handler.Handle(query, default);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(5, result.TotalItems);
        Assert.Equal(2, result.Results.Count);

        var resultsList = result.Results.ToList();

        // Verify templates in second page (items 3 and 4 from most recent)
        Assert.Equal("Template 3", resultsList[0].Name);
        Assert.Equal("Template 2", resultsList[1].Name);
    }

    [Fact]
    public async Task Handle_TemplateWithProfessionIds_ReturnsProfessionIdsCorrectly()
    {
        // Arrange
        await using var context = new InMemoryDatabaseContext();

        var profession1 = context.Professions.First(x => x.Id == 1);
        var profession2 = context.Professions.First(x => x.Id == 2);
        var profession3 = context.Professions.First(x => x.Id == 3);

        var template = await new SalariesHistoricalDataRecordTemplateFake(
                "Multi-Profession Template",
                new List<long> { profession1.Id, profession2.Id, profession3.Id })
            .PleaseAsync(context);

        context.ChangeTracker.Clear();

        var handler = new GetSalariesHistoricalDataRecordTemplatesHandler(context);

        var query = new GetSalariesHistoricalDataRecordTemplatesQuery
        {
            Page = 1,
            PageSize = 10
        };

        // Act
        var result = await handler.Handle(query, default);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result.Results);

        var returnedTemplate = result.Results.First();
        Assert.Equal(template.Name, returnedTemplate.Name);
        Assert.Equal(3, returnedTemplate.ProfessionIds.Count);
        Assert.Contains(profession1.Id, returnedTemplate.ProfessionIds);
        Assert.Contains(profession2.Id, returnedTemplate.ProfessionIds);
        Assert.Contains(profession3.Id, returnedTemplate.ProfessionIds);
    }
}