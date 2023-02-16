using System.Collections.Generic;
using MG.Utils.Kafka.Abstract;
using MG.Utils.Kafka.Implementation;
using Microsoft.Extensions.Logging;

namespace TechInterviewer.Setup;

public class KafkaHostedService : KafkaHostedServiceBase
{
    public KafkaHostedService(
        IEnumerable<IKafkaConsumer> consumers,
        ILogger<KafkaHostedService> logger,
        KafkaOptions configOptions)
        : base(consumers, logger, configOptions, delayedStartSeconds: 10)
    {
    }
}