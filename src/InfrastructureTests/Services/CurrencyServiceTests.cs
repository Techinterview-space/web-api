using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Domain.Entities.Salaries;
using Infrastructure.Currencies;
using InfrastructureTests.Fakes.Data;
using InfrastructureTests.Mocks;
using MemoryCache.Testing.Moq;
using TestUtils.Fakes;
using Xunit;

namespace InfrastructureTests.Services
{
    public class CurrencyServiceTests
    {
        [Fact]
        public async Task GetCurrenciesAsync_HasValidResposneBody_OkAsync()
        {
            // Arrange
            var configDic = new Dictionary<string, string>
            {
                { "Currencies:Url", "https://currencies.com/rates_all.xml" },
            };

            var memoryConfig = new InMemoryConfig(configDic);
            var mockedCache = Create.MockedMemoryCache();
            var mockHttpClientFactory = new HttpClientFactoryMock(FakeXml.CurrenciesXml);

            var currencyService = new CurrencyService(
                mockHttpClientFactory.Object,
                memoryConfig.Value(),
                mockedCache);

            // Act
            var currencies = await currencyService.GetCurrenciesAsync(default);

            // Assert
            Assert.Single(currencies);
            Assert.Equal(Currency.USD, currencies[0].Currency);
            Assert.Equal(446.89, currencies[0].Value);
            Assert.Equal(new DateTime(2024, 6, 7).Date, currencies[0].PubDate.Date);
        }
    }
}
