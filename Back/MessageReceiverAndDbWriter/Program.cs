using System.Text;
using System.Text.Json;
using Amazon.S3;
using Domain;
using Domain.File;
using MessageReceiverAndDbWriter;
using MongoDB.Driver;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using StackExchange.Redis;

const string tempBucketName = "tempbucket";
const string permBucketName = "permbucket";

for (;;)
{
    IConnection? messagesConnection = null;
    IConnection? fileMetasConnection = null;

    var s3Client = new AmazonS3Client("qweqweqwe", "qweqweqwe",
        new AmazonS3Config { ServiceURL = "http://minio:9000", ForcePathStyle = true });

    IConnectionMultiplexer redisConnection = ConnectionMultiplexer.Connect("cache");
    
    var mongoClient = new MongoClient("mongodb://meta:27017");
    
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
                try
                {
                    var fileId = Encoding.UTF8.GetString(ea.Body.ToArray());

                    Console.WriteLine(fileId);

                    if (!s3Client.ListBucketsAsync().GetAwaiter().GetResult().Buckets.Exists(b =>
                            b.BucketName == permBucketName))
                        s3Client.PutBucketAsync(permBucketName).GetAwaiter().GetResult();
                    s3Client.CopyObjectAsync(tempBucketName, fileId, permBucketName, fileId)
                        .GetAwaiter().GetResult();

                    var db = redisConnection.GetDatabase();
                    var data = db.StringGet(fileId);
                    if (!data.HasValue) throw new Exception();
                    Console.WriteLine(data);
                    var meta = JsonSerializer.Deserialize<SoundFileMeta>(data!) ?? throw new Exception();

                    mongoClient.GetDatabase("files").GetCollection<SoundFileMeta>("sound").InsertOne(meta);

                    channel.BasicAck(ea.DeliveryTag, false);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
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
        Console.WriteLine("Closing connections");
        messagesConnection?.Close();
        fileMetasConnection?.Close();
        s3Client.Dispose();
        await Task.Delay(1000);
    }
}