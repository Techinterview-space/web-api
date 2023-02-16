using EmailService.Integration.Core;
using EmailService.Integration.Core.Models;
using MG.Utils.Kafka.Abstract;
using Microsoft.Extensions.Logging;

namespace EmailService.Integration.Kafka.Publishers;

public class EmailMessageKafkaPublisher : KafkaPublisherBase<EmailContent>, IEmailPublisher
{
    public EmailMessageKafkaPublisher(
        ILogger<EmailMessageKafkaPublisher> logger,
        IProducer publishEndpoint,
        KafkaOptions kafkaOptions)
        : base(
            logger,
            publishEndpoint,
            kafkaOptions,
            "EmailMessageSend")
    {
    }

    protected override string BuildInfoMessageBeforeSend(EmailContent message)
        => $"Recipients count: {message.Recipients.Count}";
}