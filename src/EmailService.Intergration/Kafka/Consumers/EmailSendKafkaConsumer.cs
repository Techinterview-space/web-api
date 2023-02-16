using System;
using System.Threading.Tasks;
using EmailService.Integration.Core;
using EmailService.Integration.Core.Clients;
using EmailService.Integration.Core.Models;
using MG.Utils.Kafka.Abstract;
using MG.Utils.Kafka.Implementation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace EmailService.Integration.Kafka.Consumers;

public class EmailSendKafkaConsumer : BaseKafkaConsumer<EmailContent>
{
    private const string TopicName = "EmailMessageSend";

    private readonly ILogger<EmailSendKafkaConsumer> _logger;
    private readonly IServiceScopeFactory _scopeFactory;

    public EmailSendKafkaConsumer(
        KafkaOptions kafkaOptions,
        ILogger<EmailSendKafkaConsumer> logger,
        IServiceScopeFactory scopeFactory)
        : base(kafkaOptions.TopicByName(TopicName))
    {
        _logger = logger;
        _scopeFactory = scopeFactory;
    }

    protected override async Task ExecuteAsync(EmailContent message)
    {
        using var scope = _scopeFactory.CreateScope();
        var emailSender = scope.ServiceProvider.GetRequiredService<IEmailSender>();

        try
        {
            await emailSender.SendAsync(message);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Exception during email message consuming occured");
        }
    }
}