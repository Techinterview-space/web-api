using System.Threading.Tasks;

namespace MG.Utils.Kafka.Abstract
{
    public interface IKafkaConsumer
    {
        string Topic { get; }

        Task ConsumeAsync(string message);
    }
}