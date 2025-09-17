using System.Threading.Tasks;

namespace Project.Messaging
{
    public interface IMessageConsumer
    {
        void StartConsuming();
        void StopConsuming();
    }
}