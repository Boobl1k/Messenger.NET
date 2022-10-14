using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using MessageReceiverAndDbWriter;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

var factory = new ConnectionFactory { HostName = "rabbitmq" };
var connection = factory.CreateConnection();
var channel = connection.CreateModel();
channel.QueueDeclare(queue: "messages", durable: true, exclusive: false, autoDelete: false, arguments: null);

Console.WriteLine("Connection opened");

var context = new AppDbContext();


await Task.Run(async () =>
{
    var consumer = new EventingBasicConsumer(channel);
    consumer.Received += (ch, ea) =>
    {
        var content = Encoding.UTF8.GetString(ea.Body.ToArray());
        var message = JsonSerializer.Deserialize<Message>(content);
        Console.WriteLine(content);
        context.Messages.Add(message);
        context.SaveChanges();
        channel.BasicAck(ea.DeliveryTag, false);
    };
    for(;;)
    {
        channel.BasicConsume("messages", false, consumer);
        await Task.Delay(10);
    }
});

Console.WriteLine("Closing connection");

connection.Close();