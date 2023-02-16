using System;
using System.Text.Json;
using System.Threading.Tasks;
using MG.Utils.Kafka.Abstract;

namespace MG.Utils.Kafka.Implementation
{
    public abstract class BaseKafkaConsumer<TMessage> : IKafkaConsumer
        where TMessage : class
    {
        protected BaseKafkaConsumer(string topicName)
        {
            Topic = topicName;
        }

        protected abstract Task ExecuteAsync(TMessage message);

        public string Topic { get; }

        public async Task ConsumeAsync(string message)
        {
            if (message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            await ExecuteAsync(JsonSerializer.Deserialize<TMessage>(message));
        }
    }
}