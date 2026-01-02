using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Domain.Entities.Currencies;
using Domain.Entities.Salaries;
using Infrastructure.Currencies;
using MemoryCache.Testing.Moq;
using Microsoft.Extensions.Logging;
using Moq;
using TestUtils.Db;
using TestUtils.Fakes;
using Xunit;

namespace InfrastructureTests.Services
{
    public class CurrencyServiceTests
    {
        [Fact]
        public async Task GetCurrenciesAsync_NoSavedDatabaseEntities_ReturnsOneCurrency()
        {
            await using var context = new InMemoryDatabaseContext();

            var now = DateTime.UtcNow;
            var currenciesHttpService = new Mock<ICurrenciesHttpService>();
            currenciesHttpService
                .Setup(x => x.GetCurrenciesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Dictionary<Currency, CurrencyContent>
                {
                    {
                        Currency.USD,
                        new CurrencyContent(
                            400,
                            Currency.USD,
                            now)
                    },
                    {
                        Currency.EUR,
                        new CurrencyContent(
                            500,
                            Currency.EUR,
                            now)
                    },
                });

            using var mockedCache = Create.MockedMemoryCache();

            var currencyService = new CurrencyService(
                currenciesHttpService.Object,
                mockedCache,
                new Mock<ILogger<CurrencyService>>().Object,
                context);

            Assert.Equal(0, context.CurrencyCollections.Count());

            context.ChangeTracker.Clear();
            var currency = await currencyService.GetCurrencyOrNullAsync(
                Currency.USD,
                CancellationToken.None);

            // Assert
            Assert.NotNull(currency);
            Assert.Equal(Currency.USD, currency.Currency);
            Assert.Equal(400, currency.Value);
            Assert.Equal(now, currency.PubDate);

            Assert.Equal(1, context.CurrencyCollections.Count());
            currenciesHttpService
                .Verify(
                    x => x.GetCurrenciesAsync(It.IsAny<CancellationToken>()),
                    Times.Once);
        }

        [Fact]
        public async Task GetAllCurrenciesAsync_NoSavedDatabaseRecord_ReturnsAllCurrencies()
        {
            await using var context = new InMemoryDatabaseContext();
            var now = DateTime.UtcNow;

            var currenciesHttpService = new Mock<ICurrenciesHttpService>();
            currenciesHttpService
                .Setup(x => x.GetCurrenciesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Dictionary<Currency, CurrencyContent>
                {
                    {
                        Currency.USD,
                        new CurrencyContent(
                            400,
                            Currency.USD,
                            now)
                    },
                    {
                        Currency.EUR,
                        new CurrencyContent(
                            500,
                            Currency.EUR,
                            now)
                    },
                });

            using var mockedCache = Create.MockedMemoryCache();

            var currencyService = new CurrencyService(
                currenciesHttpService.Object,
                mockedCache,
                new Mock<ILogger<CurrencyService>>().Object,
                context);

            var collection1 = new CurrenciesCollectionFake(DateTime.UtcNow.AddDays(-2)).Please(context);
            var collection2 = new CurrenciesCollectionFake(DateTime.UtcNow.AddDays(-3)).Please(context);
            var collection3 = new CurrenciesCollectionFake(DateTime.UtcNow.AddDays(-4)).Please(context);

            Assert.Equal(3, context.CurrencyCollections.Count());

            context.ChangeTracker.Clear();
            var currencies = await currencyService.GetAllCurrenciesAsync(default);

            Assert.NotNull(currencies);
            Assert.Equal(3, currencies.Count);
            Assert.Contains(currencies, x => x.Currency == Currency.USD);
            Assert.Contains(currencies, x => x.Currency == Currency.EUR);
            Assert.Contains(currencies, x => x.Currency == Currency.KZT);

            Assert.Equal(collection1.CurrencyDate, currencies[0].PubDate);
            Assert.NotEqual(collection2.CurrencyDate, currencies[0].PubDate);
            Assert.NotEqual(collection3.CurrencyDate, currencies[0].PubDate);

            // Assert
            currenciesHttpService
                .Verify(
                    x => x.GetCurrenciesAsync(It.IsAny<CancellationToken>()),
                    Times.Never);
        }

        [Fact]
        public async Task RefetchServiceCurrenciesAsync_HttpServiceReturnsRecords_NewRecordCreatedInDatabase()
        {
            // Arrange
            await using var context = new InMemoryDatabaseContext();
            var now = DateTime.UtcNow;

            var currenciesHttpService = new Mock<ICurrenciesHttpService>();
            currenciesHttpService
                .Setup(x => x.GetCurrenciesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Dictionary<Currency, CurrencyContent>
                {
                    {
                        Currency.USD,
                        new CurrencyContent(
                            400,
                            Currency.USD,
                            now)
                    },
                    {
                        Currency.EUR,
                        new CurrencyContent(
                            500,
                            Currency.EUR,
                            now)
                    },
                });

            using var mockedCache = Create.MockedMemoryCache();

            var currencyService = new CurrencyService(
                currenciesHttpService.Object,
                mockedCache,
                new Mock<ILogger<CurrencyService>>().Object,
                context);

            Assert.Equal(0, context.CurrencyCollections.Count());

            // Act
            await currencyService.RefetchServiceCurrenciesAsync(CancellationToken.None);

            // Assert
            Assert.Equal(1, context.CurrencyCollections.Count());
            var savedCollection = context.CurrencyCollections.First();
            Assert.Equal(now.Date, savedCollection.CurrencyDate.Date);
            Assert.Equal(2, savedCollection.Currencies.Count);

            currenciesHttpService
                .Verify(
                    x => x.GetCurrenciesAsync(It.IsAny<CancellationToken>()),
                    Times.Once);
        }

        [Fact]
        public async Task RefetchServiceCurrenciesAsync_HttpServiceReturnsNoRecords_NoRecordCreatedInDatabase()
        {
            // Arrange
            await using var context = new InMemoryDatabaseContext();

            var currenciesHttpService = new Mock<ICurrenciesHttpService>();
            currenciesHttpService
                .Setup(x => x.GetCurrenciesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Dictionary<Currency, CurrencyContent>());

            using var mockedCache = Create.MockedMemoryCache();

            var currencyService = new CurrencyService(
                currenciesHttpService.Object,
                mockedCache,
                new Mock<ILogger<CurrencyService>>().Object,
                context);

            Assert.Equal(0, context.CurrencyCollections.Count());

            // Act
            await currencyService.RefetchServiceCurrenciesAsync(CancellationToken.None);

            // Assert
            Assert.Equal(0, context.CurrencyCollections.Count());

            currenciesHttpService
                .Verify(
                    x => x.GetCurrenciesAsync(It.IsAny<CancellationToken>()),
                    Times.Once);
        }

        [Fact]
        public async Task RefetchServiceCurrenciesAsync_SomeDbRecordsForPrevousDate_NewRecordCreatedInDatabase()
        {
            // Arrange
            await using var context = new InMemoryDatabaseContext();
            var now = DateTime.UtcNow;

            var record1 = new CurrenciesCollectionFake(now.Date.AddDays(-1)).Please(context);
            var record2 = new CurrenciesCollectionFake(now.Date.AddDays(-2)).Please(context);

            var currenciesHttpService = new Mock<ICurrenciesHttpService>();
            currenciesHttpService
                .Setup(x => x.GetCurrenciesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Dictionary<Currency, CurrencyContent>
                {
                    {
                        Currency.USD,
                        new CurrencyContent(
                            400,
                            Currency.USD,
                            now)
                    },
                    {
                        Currency.EUR,
                        new CurrencyContent(
                            500,
                            Currency.EUR,
                            now)
                    },
                });

            using var mockedCache = Create.MockedMemoryCache();

            var currencyService = new CurrencyService(
                currenciesHttpService.Object,
                mockedCache,
                new Mock<ILogger<CurrencyService>>().Object,
                context);

            Assert.Equal(2, context.CurrencyCollections.Count());

            // Act
            await currencyService.RefetchServiceCurrenciesAsync(CancellationToken.None);

            // Assert
            Assert.Equal(3, context.CurrencyCollections.Count());
            var savedCollection = context.CurrencyCollections.Last();

            Assert.NotEqual(record1.CurrencyDate, savedCollection.CurrencyDate);
            Assert.NotEqual(record2.CurrencyDate, savedCollection.CurrencyDate);

            Assert.Equal(now.Date, savedCollection.CurrencyDate.Date);
            Assert.Equal(2, savedCollection.Currencies.Count);

            currenciesHttpService
                .Verify(
                    x => x.GetCurrenciesAsync(It.IsAny<CancellationToken>()),
                    Times.Once);
        }

        [Fact]
        public async Task RefetchServiceCurrenciesAsync_ExistingRecordForSameDate_NoNewRecordCreated()
        {
            // Arrange
            await using var context = new InMemoryDatabaseContext();
            var now = DateTime.UtcNow;

            var existingCollection = new CurrenciesCollectionFake(now.Date).Please(context);

            var currenciesHttpService = new Mock<ICurrenciesHttpService>();
            currenciesHttpService
                .Setup(x => x.GetCurrenciesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Dictionary<Currency, CurrencyContent>
                {
                    {
                        Currency.USD,
                        new CurrencyContent(
                            400,
                            Currency.USD,
                            now)
                    },
                    {
                        Currency.EUR,
                        new CurrencyContent(
                            500,
                            Currency.EUR,
                            now)
                    },
                });

            using var mockedCache = Create.MockedMemoryCache();

            var currencyService = new CurrencyService(
                currenciesHttpService.Object,
                mockedCache,
                new Mock<ILogger<CurrencyService>>().Object,
                context);

            Assert.Equal(1, context.CurrencyCollections.Count());

            context.ChangeTracker.Clear();

            // Act
            await currencyService.RefetchServiceCurrenciesAsync(CancellationToken.None);

            // Assert
            Assert.Equal(1, context.CurrencyCollections.Count());
            var savedCollection = context.CurrencyCollections.First();
            Assert.Equal(existingCollection.Id, savedCollection.Id);
            Assert.Equal(now.Date, savedCollection.CurrencyDate);

            currenciesHttpService
                .Verify(
                    x => x.GetCurrenciesAsync(It.IsAny<CancellationToken>()),
                    Times.Never);
        }
    }
}
