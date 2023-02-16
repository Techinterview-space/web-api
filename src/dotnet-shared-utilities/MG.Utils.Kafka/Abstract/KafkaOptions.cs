using System;
using Confluent.Kafka;
using Microsoft.Extensions.Configuration;

namespace MG.Utils.Kafka.Abstract;

public record KafkaOptions
{
    private readonly IConfigurationSection _settings;
    private readonly IConfigurationSection _topics;

    public string Host { get; }

    public KafkaOptions(IConfiguration configuration)
    {
        _settings = configuration.GetSection("MessageBroker");
        _topics = _settings.GetSection("Topics");
        Host = _settings["Host"];
    }

    public string TopicByName(string name)
    {
        return _topics[name];
    }

    public ProducerConfig ProducerConfig()
    {
        var host = _settings["Host"];

        if (!int.TryParse(_settings["MessageTimeoutMs"], out var messageTimeout))
        {
            throw new InvalidOperationException("MessageTimeoutMs must be an integer");
        }

        return new ProducerConfig
        {
            BootstrapServers = host,
            MessageTimeoutMs = messageTimeout
        };
    }

    public ConsumerConfig ConsumerConfig()
    {
        if (_settings["GroupId"] == null)
        {
            throw new ArgumentException("GroupId must be set");
        }

        return new ConsumerConfig(ProducerConfig())
        {
            GroupId = _settings["GroupId"],
            AllowAutoCreateTopics = true
        };
    }
}