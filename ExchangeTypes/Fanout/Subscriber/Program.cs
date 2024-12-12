using System.Text;
using Microsoft.Extensions.Configuration;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

internal class Program
{
    private static async Task Main(string[] args)
    {
        var configuration = new ConfigurationBuilder()
                    .SetBasePath(AppContext.BaseDirectory) 
                    .AddJsonFile("Settings.json", optional: true, reloadOnChange: true) 
                    .Build();

        var factory = new ConnectionFactory();
        factory.Uri = new Uri(configuration["AppSettings:Connection"]);

        using var connection = await factory.CreateConnectionAsync();
        using var channel = await connection.CreateChannelAsync();

        QueueDeclareOk queueDeclareOk= await channel.QueueDeclareAsync();
        var queueName =  queueDeclareOk.QueueName;

        await channel.QueueBindAsync(queueName,"logs-fanout","",null);

        //how to share message to subscribes by count.
        await channel.BasicQosAsync(0,1,false);

        Console.WriteLine($"Waiting for logs");
    
        var consumer = new AsyncEventingBasicConsumer(channel);
        consumer.ReceivedAsync += (model, e) =>
        {
            Thread.Sleep(1500);
            var body = e.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);
            Console.WriteLine($" [x] Received {message}");

            //the message is deleted after process complated
            channel.BasicAckAsync(e.DeliveryTag,false);

            return Task.CompletedTask;
        };

        await channel.BasicConsumeAsync(queueName, autoAck: true, consumer: consumer);
        

        Console.ReadLine();

    }
}