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
using TechInterviewer.Features.Telegram.ProcessMessage;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using TestUtils.Db;
using TestUtils.Fakes;
using Web.Api.Tests.Mocks;
using Web.Api.Tests.Telegram.Data.AssertData;
using Xunit;

namespace Web.Api.Tests.Telegram
{
    public class ProcessTelegramMessageCommandTests
    {
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

            Assert.Equal(AssertData.ProcessMessage1, replyText);
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

            Assert.Equal(AssertData.ProcessMessage2, replyText);
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
