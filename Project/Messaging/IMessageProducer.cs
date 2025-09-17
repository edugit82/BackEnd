using System.Threading.Tasks;

namespace Project.Messaging
{
    public interface IMessageProducer
    {
        void PublishMessage<T>(T message, string routingKey);
        void Publish(string key, string value, string routingKey);
    }
}