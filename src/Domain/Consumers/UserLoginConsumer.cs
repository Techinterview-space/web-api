using System.Threading.Tasks;
using Domain.Consumers.Contract.Messages;
using Domain.Database;
using MG.Utils.Kafka.Abstract;
using MG.Utils.Kafka.Implementation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Domain.Consumers;

public class UserLoginConsumer : BaseKafkaConsumer<UserLoginMessage>
{
    private readonly ILogger<UserLoginConsumer> _logger;
    private readonly IServiceScopeFactory _scopeFactory;

    public UserLoginConsumer(IServiceScopeFactory scopeFactory, ILogger<UserLoginConsumer> logger, KafkaOptions kafkaOptions)
        : base(kafkaOptions.TopicByName(UserLoginMessage.TopicName))
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(UserLoginMessage message)
    {
        using var scope = _scopeFactory.CreateScope();
        var databaseContext = scope.ServiceProvider.GetRequiredService<DatabaseContext>();

        var user = await databaseContext.Users
            .FirstOrDefaultAsync(x => x.IdentityId == message.IdentityId);

        if (user == null)
        {
            _logger.LogError($"'User with IdentityId {message.IdentityId} was not found'");
            return;
        }

        user.RenewLastLoginTime();
        await databaseContext.SaveChangesAsync();
    }
}