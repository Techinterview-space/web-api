using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Domain.Entities.Enums;
using Domain.Entities.Salaries;
using Domain.Enums;
using Infrastructure.Database;
using MemoryCache.Testing.Moq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using TestUtils.Db;
using TestUtils.Fakes;
using Web.Api.Features.Telegram.ProcessMessage;
using Web.Api.Tests.Mocks;
using Xunit;

namespace Web.Api.Tests.Telegram
{
    public class ProcessTelegramMessageCommandTests
    {
        public const string ProcessMessage1 = "Зарплаты специалистов IT в Казахстане по грейдам:\n\nДжуны: <b>2,222</b> тг. (~5$)\nМиддлы: <b>5,556</b> тг. (~12$)\nСеньоры: <b>8,889</b> тг. (~20$)\nЛиды: <b>11,111</b> тг. (~25$)<em>\n\nРассчитано на основе 12 анкет(ы)</em>\n<em>Подробно на сайте <a href=\"https://techinterview.space/salaries\">techinterview.space/salaries</a></em>";
        public const string ProcessMessage2 = "Пока никто не оставлял информации о зарплатах.\n\n<em>Посмотреть зарплаты по другим специальностям можно на сайте <a href=\"https://techinterview.space/salaries\">techinterview.space/salaries</a></em>";

        [Fact]
        public async Task TestBasicHandle_WithData()
        {
            await using var context = new InMemoryDatabaseContext();
            var user = await new FakeUser(Role.Interviewer).PleaseAsync(context);

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

            var logger = new Mock<ILogger<ProcessTelegramMessageHandler>>();
            var currencyService = new CurrenciesServiceFake();

            var mockedCache = Create.MockedMemoryCache();
            var telegramBotClient = new Mock<ITelegramBotClient>();

            var processTelegramMessageHandler = new ProcessTelegramMessageHandler(
                logger.Object,
                currencyService,
                context,
                mockedCache,
                new GlobalFake());

            var mockUpdate = new Mock<Update>();

            mockUpdate.Object.Message = new Message()
            {
                Chat = new Chat()
                {
                    Type = ChatType.Private,
                }
            };

            var replyText = await processTelegramMessageHandler.Handle(
                new ProcessTelegramMessageCommand(
                    telegramBotClient.Object, mockUpdate.Object), default);

            Assert.Equal(ProcessMessage1, replyText);
        }

        [Fact]
        public async Task TestBasicHandle_WithNoData()
        {
            await using var context = new InMemoryDatabaseContext();
            var user = await new FakeUser(Role.Interviewer).PleaseAsync(context);

            var logger = new Mock<ILogger<ProcessTelegramMessageHandler>>();
            var currencyService = new CurrenciesServiceFake();

            var mockedCache = Create.MockedMemoryCache();
            var telegramBotClient = new Mock<ITelegramBotClient>();

            var processTelegramMessageHandler = new ProcessTelegramMessageHandler(
                logger.Object,
                currencyService,
                context,
                mockedCache,
                new GlobalFake());

            var mockUpdate = new Mock<Update>();
            mockUpdate.Object.Message = new Message()
            {
                Chat = new Chat()
                {
                    Type = ChatType.Private,
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
