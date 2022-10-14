using Domain;

namespace Presentation.RabbitMQ.Producer;

public interface IMessageProducer
{
    void SendMessage(Message message);
}