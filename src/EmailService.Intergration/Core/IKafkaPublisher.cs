using System.Threading.Tasks;

namespace EmailService.Integration.Core;

public interface IKafkaPublisher<in TMessage>
    where TMessage : class, new()
{
    Task PublishAsync(TMessage message);
}