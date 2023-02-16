using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Confluent.Kafka;
using MG.Utils.Kafka.Abstract;

namespace MG.Utils.Kafka;

public class KafkaTopicsCollection : IEnumerable<TopicMetadata>
{
    private readonly KafkaOptions _options;

    public KafkaTopicsCollection(KafkaOptions options)
    {
        _options = options;
    }

    public IEnumerator<TopicMetadata> GetEnumerator()
    {
        var adminConfig = new AdminClientConfig()
        {
            BootstrapServers = _options.Host
        };

        using var adminClient = new AdminClientBuilder(adminConfig).Build();
        var metadata = adminClient.GetMetadata(TimeSpan.FromSeconds(10));
        return metadata.Topics.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}