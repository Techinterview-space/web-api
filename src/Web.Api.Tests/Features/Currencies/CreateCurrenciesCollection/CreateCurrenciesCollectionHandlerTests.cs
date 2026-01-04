using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Domain.Entities.Salaries;
using Domain.Validation.Exceptions;
using Microsoft.EntityFrameworkCore;
using TestUtils.Db;
using TestUtils.Fakes;
using Web.Api.Features.Currencies.CreateCurrenciesCollection;
using Xunit;

namespace Web.Api.Tests.Features.Currencies.CreateCurrenciesCollection;

public class CreateCurrenciesCollectionHandlerTests
{
    [Fact]
    public async Task Handle_NoExistingRecordForDate_ValidData_CreatesNewRecord()
    {
        // Arrange
        await using var context = new InMemoryDatabaseContext();
        var handler = new CreateCurrenciesCollectionHandler(context);

        var currencyDate = new DateTime(2024, 1, 15);
        var request = new CreateCurrenciesCollectionRequest
        {
            CurrencyDate = currencyDate,
            Currencies = new Dictionary<Currency, double>
            {
                { Currency.USD, 450.50 },
                { Currency.EUR, 520.75 },
                { Currency.KZT, 1.0 }
            }
        };

        Assert.Equal(0, context.CurrencyCollections.Count());

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(currencyDate, result.CurrencyDate);
        Assert.Equal(3, result.Currencies.Count);

        var createdRecord = await context.CurrencyCollections
            .FirstOrDefaultAsync(x => x.CurrencyDate == currencyDate);

        Assert.NotNull(createdRecord);
        Assert.Equal(currencyDate, createdRecord.CurrencyDate);
        Assert.Equal(3, createdRecord.Currencies.Count);
        Assert.Equal(450.50, createdRecord.Currencies[Currency.USD]);
        Assert.Equal(520.75, createdRecord.Currencies[Currency.EUR]);
        Assert.Equal(1.0, createdRecord.Currencies[Currency.KZT]);
        Assert.Equal(1, context.CurrencyCollections.Count());
    }

    [Fact]
    public async Task Handle_ExistingRecordForDate_ValidData_ThrowsBadRequestException()
    {
        // Arrange
        await using var context = new InMemoryDatabaseContext();
        var currencyDate = new DateTime(2024, 1, 15);

        var existingRecord = new CurrenciesCollectionFake(currencyDate).Please(context);

        var handler = new CreateCurrenciesCollectionHandler(context);

        var request = new CreateCurrenciesCollectionRequest
        {
            CurrencyDate = currencyDate,
            Currencies = new Dictionary<Currency, double>
            {
                { Currency.USD, 460.00 },
                { Currency.EUR, 530.00 }
            }
        };

        Assert.Equal(1, context.CurrencyCollections.Count());

        // Act & Assert
        var exception = await Assert.ThrowsAsync<BadRequestException>(() =>
            handler.Handle(request, CancellationToken.None));

        Assert.Contains("already exists", exception.Message);

        var allRecords = context.CurrencyCollections.ToList();
        Assert.Single(allRecords);
        Assert.Equal(existingRecord.Id, allRecords[0].Id);
    }

    [Fact]
    public async Task Handle_NoCurrencies_ThrowsBadRequestException()
    {
        // Arrange
        await using var context = new InMemoryDatabaseContext();
        var handler = new CreateCurrenciesCollectionHandler(context);

        var request = new CreateCurrenciesCollectionRequest
        {
            CurrencyDate = new DateTime(2024, 1, 15),
            Currencies = new Dictionary<Currency, double>()
        };

        Assert.Equal(0, context.CurrencyCollections.Count());

        // Act & Assert
        var exception = await Assert.ThrowsAsync<BadRequestException>(() =>
            handler.Handle(request, CancellationToken.None));

        Assert.Contains("at least one currency", exception.Message);
        Assert.Equal(0, context.CurrencyCollections.Count());
    }

    [Fact]
    public async Task Handle_FutureDate_ThrowsBadRequestException()
    {
        // Arrange
        await using var context = new InMemoryDatabaseContext();
        var handler = new CreateCurrenciesCollectionHandler(context);

        var futureDate = DateTime.UtcNow.Date.AddDays(30);
        var request = new CreateCurrenciesCollectionRequest
        {
            CurrencyDate = futureDate,
            Currencies = new Dictionary<Currency, double>
            {
                { Currency.USD, 450.00 },
                { Currency.EUR, 520.00 }
            }
        };

        Assert.Equal(0, context.CurrencyCollections.Count());

        // Act & Assert
        var exception = await Assert.ThrowsAsync<BadRequestException>(() =>
            handler.Handle(request, CancellationToken.None));

        Assert.Contains("future", exception.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(0, context.CurrencyCollections.Count());
    }
}
