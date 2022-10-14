using System.Net;
using System.Text;
using Back.Entities;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;


var listener = new HttpListener();
listener.Prefixes.Add("http://localhost:8888/");
listener.Start();
Console.WriteLine("Ожидание подключений...");

while (true)
{
    var context = await listener.GetContextAsync();
    _ = Task.Run(async () =>
    {
        var request = context.Request;
        var response = context.Response;

        
        var factory = new ConnectionFactory { HostName = "localhost" };
        var connection = factory.CreateConnection();
        using var channel = connection.CreateModel();
        channel.QueueDeclare(queue: "messages",
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null);

        channel.BasicQos(prefetchSize: 0, prefetchCount: 3, global: false);

        var consumer = new EventingBasicConsumer(channel);
        consumer.Received += (_, eventArgs) =>
        {
            var body = eventArgs.Body.ToArray();
            var msg = Encoding.UTF8.GetString(body);
            Console.WriteLine(msg);
        };
        channel.BasicConsume(queue: "messages", autoAck: true, consumer: consumer);
        
        
        const string responseString = "<html><head><meta charset='utf8'></head><body>Привет мир!</body></html>";
        var buffer = Encoding.UTF8.GetBytes(responseString);
        response.ContentLength64 = buffer.Length;
        var output = response.OutputStream;
        output.Write(buffer, 0, buffer.Length);
        output.Close();
    });
}

// class MessageReceiver
// {
//     void ReceiveMessage()
//     {
//
//     }
// }