using System.Threading.Tasks;

namespace MG.Utils.Kafka.Abstract
{
    public interface IProducer
    {
        Task PublishAsync<T>(string topic, T message);
    }
}