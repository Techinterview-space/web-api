using System;
using System.Threading.Tasks;
using MG.Utils.Kafka.Abstract;
using MG.Utils.Kafka.Exceptions;
using Microsoft.Extensions.Logging;

namespace EmailService.Integration.Core;

public abstract class KafkaPublisherBase<TMessage> : IKafkaPublisher<TMessage>
    where TMessage : class, new()
{
    protected ILogger Logger { get; }

    protected IProducer PublishEndpoint { get; }

    protected string Topic { get; }

    protected KafkaPublisherBase(
        ILogger logger,
        IProducer publishEndpoint,
        KafkaOptions kafkaOptions,
        string topicSettingsKey)
    {
        Logger = logger;
        PublishEndpoint = publishEndpoint;
        Topic = kafkaOptions.TopicByName(topicSettingsKey);
    }

    public async Task PublishAsync(TMessage message)
    {
        try
        {
            Logger.LogInformation("Publish to topic {Topic}. {Message}", Topic, BuildInfoMessageBeforeSend(message));
            await PublishEndpoint.PublishAsync(Topic, message);
        }
        catch (Exception exception)
        {
            throw new CannotPublishMessageException<TMessage>(exception);
        }
    }

    protected abstract string BuildInfoMessageBeforeSend(TMessage message);
}