using System;
using System.Linq;
using MG.Utils.Kafka.Abstract;
using Microsoft.Extensions.DependencyInjection;

namespace MG.Utils.Kafka;

public static class KafkaExtensions
{
    public static IServiceCollection AddConsumer<T>(
        this IServiceCollection services)
        where T : class, IKafkaConsumer
    {
        if (services.Any(x => x.ServiceType == typeof(T)))
        {
            throw new ArgumentException($"You're trying to register consumer {typeof(T).Name} again");
        }

        return services.AddTransient<IKafkaConsumer, T>();
    }
}