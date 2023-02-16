using System;
using System.Text.Json;
using System.Threading.Tasks;
using Confluent.Kafka;
using MG.Utils.Kafka.Abstract;

namespace MG.Utils.Kafka.Implementation
{
    public class KafkaProducer : IProducer
    {
        private readonly ProducerConfig _config;

        public KafkaProducer(KafkaOptions configuration)
        {
            _config = configuration.ProducerConfig();
        }

        public async Task PublishAsync<T>(string topic, T message)
        {
            if (message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            using var producer = new ProducerBuilder<string, string>(_config).Build();
            var value = JsonSerializer.Serialize(message);

            await producer.ProduceAsync(topic, new Message<string, string> { Value = value });

            producer.Flush();
        }
    }
}