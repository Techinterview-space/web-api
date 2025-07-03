using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Domain.Entities.Enums;
using Domain.Entities.Salaries;
using Domain.Enums;
using Infrastructure.Database;
using MemoryCache.Testing.Moq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using TestUtils.Db;
using TestUtils.Fakes;
using TestUtils.Mocks;
using Web.Api.Features.Telegram;
using Web.Api.Features.Telegram.ProcessSalariesRelatedMessage;
using Web.Api.Tests.Mocks;
using Xunit;

namespace Web.Api.Tests.Telegram
{
    public class ProcessSalariesRelatedTelegramMessageHandlerTests
    {
        public const string ProcessMessage1 = "Зарплаты специалистов IT в Казахстане по грейдам:\n\n" +
                                              "Джуны: <b>2,222</b> тг. (~5$)\n" +
                                              "Миддлы: <b>5,556</b> тг. (~12$)\n" +
                                              "Сеньоры: <b>8,889</b> тг. (~20$)\n" +
                                              "Лиды: <b>11,111</b> тг. (~25$)<em>\n\n" +
                                              "Рассчитано на основе 12 анкет(ы)</em>\n" +
                                              "<em>Разные графики и фильтры доступны по ссылке <a href=\"https://techinterview.space/salaries?utm_source=0&utm_campaign=telegram-reply\">techinterview.space/salaries</a></em>\n\n" +
                                              "#статистика_зарплат";

        public const string ProcessMessage2 = "Пока никто не оставлял информации о зарплатах.\n\n<em>Посмотреть зарплаты по другим специальностям можно на сайте <a href=\"https://techinterview.space/salaries?utm_source=0&utm_campaign=telegram-reply\">techinterview.space/salaries</a></em>";

        [Fact]
        public async Task TestBasicHandle_WithData()
        {
            await using var context = new InMemoryDatabaseContext();
            var user = await new UserFake(Role.Interviewer).PleaseAsync(context);

            await GenerateUserSalaries(
                context,
                user,
                DeveloperGrade.Junior,
                [1111.11, 2222.22, 3333.33]);
            await GenerateUserSalaries(
                context,
                user,
                DeveloperGrade.Middle,
                [4444.44, 5555.55, 6666.66]);
            await GenerateUserSalaries(
                context,
                user,
                DeveloperGrade.Senior,
                [7777.77, 8888.88, 9999.99]);
            await GenerateUserSalaries(
                context,
                user,
                DeveloperGrade.Lead,
                [10111.10, 11111.11, 12222.12]);

            var logger = new Mock<ILogger<ProcessSalariesRelatedTelegramMessageHandler>>();
            var currencyService = new CurrenciesServiceFake();

            using var mockedCache = Create.MockedMemoryCache();
            var telegramBotClient = new Mock<ITelegramBotClient>();
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(
                    new Dictionary<string, string>
                    {
                        { "Telegram:AdminUsername", "maximgorbatyuk" },
                    })
                .Build();

            var processTelegramMessageHandler = new ProcessSalariesRelatedTelegramMessageHandler(
                logger.Object,
                currencyService,
                context,
                mockedCache,
                new GlobalFake(),
                new ProfessionsCacheServiceFake(context),
                configuration);

            var mockUpdate = new Mock<Update>();

            mockUpdate.Object.Message = new Message()
            {
                Chat = new Chat()
                {
                    Type = ChatType.Private,
                    Username = "test_username",
                },
                From = new User
                {
                    Username = "test_username",
                }
            };

            var replyText = await processTelegramMessageHandler.Handle(
                new ProcessTelegramMessageCommand(
                    telegramBotClient.Object,
                    mockUpdate.Object),
                default);

            Assert.Equal(ProcessMessage1, replyText);
        }

        [Fact]
        public async Task TestBasicHandle_WithNoData()
        {
            await using var context = new InMemoryDatabaseContext();
            var user = await new UserFake(Role.Interviewer).PleaseAsync(context);

            var logger = new Mock<ILogger<ProcessSalariesRelatedTelegramMessageHandler>>();
            var currencyService = new CurrenciesServiceFake();

            using var mockedCache = Create.MockedMemoryCache();
            var telegramBotClient = new Mock<ITelegramBotClient>();

            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(
                    new Dictionary<string, string>
                    {
                        { "Telegram:AdminUsername", "maximgorbatyuk" },
                    })
                .Build();

            var processTelegramMessageHandler = new ProcessSalariesRelatedTelegramMessageHandler(
                logger.Object,
                currencyService,
                context,
                mockedCache,
                new GlobalFake(),
                new ProfessionsCacheServiceFake(context),
                configuration);

            var mockUpdate = new Mock<Update>();
            mockUpdate.Object.Message = new Message()
            {
                Chat = new Chat()
                {
                    Type = ChatType.Private,
                    Username = "test_username",
                },
                From = new User
                {
                    Username = "test_username",
                }
            };

            var replyText = await processTelegramMessageHandler.Handle(
                new ProcessTelegramMessageCommand(
                    telegramBotClient.Object, mockUpdate.Object), default);

            Assert.Equal(ProcessMessage2, replyText);
        }

        private async Task GenerateUserSalaries(
             DatabaseContext context,
             Domain.Entities.Users.User user,
             DeveloperGrade grade,
             IEnumerable<double> salaryValues)
        {
            const CompanyType company = CompanyType.Local;

            var developerProfession = await context.Professions.FirstAsync(x => x.Id == (long)UserProfessionEnum.Developer);
            foreach (var salaryValue in salaryValues)
            {
                await context.SaveAsync(new UserSalaryFake(
                        user,
                        value: salaryValue,
                        grade: grade,
                        company: company,
                        createdAt: DateTimeOffset.Now.AddDays(-1),
                        professionOrNull: developerProfession)
                    .AsDomain());
            }
        }
    }
}
