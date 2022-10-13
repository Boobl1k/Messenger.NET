using Back.Entities;

namespace Back.RabbitMQ.Consumer;

public interface IMessageConsumer
{
    void ReceiveMessage(Message message);
}