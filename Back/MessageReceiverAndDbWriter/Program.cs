using System.Text;
using System.Text.Json;
using Domain;
using MessageReceiverAndDbWriter;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

for (;;)
{
    IConnection? messagesConnection = null;
    IConnection? fileMetasConnection = null;
    try
    {
        var factory = new ConnectionFactory { HostName = "rabbitmq" };

        {
            messagesConnection = factory.CreateConnection();
            var channel = messagesConnection.CreateModel();
            channel.QueueDeclare(queue: "messages", durable: true, exclusive: false, autoDelete: false,
                arguments: null);

            Console.WriteLine("Messages connection opened");

            var context = new AppDbContext();


            var consumer = new EventingBasicConsumer(channel);
            consumer.Received += (ch, ea) =>
            {
                var content = Encoding.UTF8.GetString(ea.Body.ToArray());
                var message = JsonSerializer.Deserialize<Message>(content);
                if (message is null)
                {
                    Console.WriteLine($"Broken message: '{content}'");
                    return;
                }

                Console.WriteLine($"New message: {message.UserName} says '{message.Text}'");
                context.Messages.Add(message);
                context.SaveChanges();
                channel.BasicAck(ea.DeliveryTag, false);
            };
            channel.BasicConsume("messages", false, consumer);
        }

        {
            fileMetasConnection = factory.CreateConnection();
            var channel = fileMetasConnection.CreateModel();
            channel.QueueDeclare(queue: "fileMetas", durable: true, exclusive: false, autoDelete: false,
                arguments: null);
            
            Console.WriteLine("File metas connection opened");

            var consumer = new EventingBasicConsumer(channel);
            consumer.Received += (ch, ea) =>
            {
                var content = Encoding.UTF8.GetString(ea.Body.ToArray());

                Console.WriteLine(content);
                channel.BasicAck(ea.DeliveryTag, false);
            };
            channel.BasicConsume("fileMetas", false, consumer);
        }

        for (;;)
        {
        }
    }
    catch (Exception e)
    {
        Console.WriteLine(e.Message);
    }
    finally
    {
        Console.WriteLine("Closing connection");
        messagesConnection?.Close();
        fileMetasConnection?.Close();
        await Task.Delay(1000);
    }
}