using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Confluent.Kafka;
using MG.Utils.Kafka.Abstract;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace MG.Utils.Kafka.Implementation
{
    public abstract class KafkaHostedServiceBase : IHostedService
    {
        protected ILogger Logger { get; }

        protected KafkaOptions ConfigOptions { get; }

        protected bool Cancelled { get; private set; }

        protected IReadOnlyCollection<IKafkaConsumer> Consumers { get; }

        private readonly TimeSpan _delayedStart;

        protected KafkaHostedServiceBase(
            IEnumerable<IKafkaConsumer> consumers,
            ILogger logger,
            KafkaOptions configOptions,
            int delayedStartSeconds)
        {
            Consumers = consumers.ToArray();
            Logger = logger;
            ConfigOptions = configOptions;
            _delayedStart = TimeSpan.FromSeconds(delayedStartSeconds);
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            Logger.LogInformation($"Consuming will start 10 secs later");

            // Delayed start
            await Task.Delay(_delayedStart, cancellationToken);

            var tasks = Consumers
                .Select(x =>
                {
                    Logger.LogInformation($"Consumer for topic: {x.Topic} registered");
                    return Task.Run(() => ConsumeInternalAsync(x, cancellationToken), cancellationToken);
                })
                .ToArray();

            await Task.WhenAll(tasks);
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            Cancelled = cancellationToken.IsCancellationRequested;
            return Task.CompletedTask;
        }

        private async Task ConsumeInternalAsync(IKafkaConsumer kafkaConsumer, CancellationToken cancellationToken)
        {
            var builder = new ConsumerBuilder<string, string>(ConfigOptions.ConsumerConfig());

            using IConsumer<string, string> consumer = builder.Build();

            try
            {
                consumer.Subscribe(kafkaConsumer.Topic);

                try
                {
                    while (!Cancelled && !cancellationToken.IsCancellationRequested)
                    {
                        ConsumeResult<string, string> consumed = consumer.Consume(cancellationToken);

                        Logger.LogInformation($"{kafkaConsumer.GetType().Name}: consumed");

                        await kafkaConsumer.ConsumeAsync(consumed.Message.Value);
                    }
                }
                catch (ConsumeException e)
                {
                    Logger.LogError(e, kafkaConsumer.Topic +
                                        $"\r\nAn exception during consuming\r\n" +
                                        $"Reason: {e.Error.Reason}\r\n" +
                                        $"Consumer is being closed");
                }
            }
            catch (Exception exception)
            {
                Logger.LogError(
                    exception,
                    $"Topic: {kafkaConsumer.Topic}\r\n" +
                    $"An exception during consuming\r\n");
            }
            finally
            {
                Logger.LogInformation($"{kafkaConsumer.Topic}. Consumer is being closed");
                consumer.Close();
            }
        }
    }
}