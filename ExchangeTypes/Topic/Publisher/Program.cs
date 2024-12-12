using System.Linq;
using System.Text;
using Microsoft.Extensions.Configuration;
using RabbitMQ.Client;

internal class Program
{
    public enum LogNames
    {
        Critical=1,
        Error=2,
        Warning=3,
        Info=4

    }

    private static async Task Main(string[] args)
    {
         var configuration = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory) 
            .AddJsonFile("Settings.json", optional: true, reloadOnChange: true) 
            .Build();

        var factory = new ConnectionFactory();

        var test = configuration["AppSettings:Connection"];

        factory.Uri = new Uri(configuration["AppSettings:Connection"]);

        using var connection = await factory.CreateConnectionAsync();
        var channel = await connection.CreateChannelAsync();
        
        //Exchange declarated as Topic
        await channel.ExchangeDeclareAsync("logs-topic",type: ExchangeType.Topic,durable:true);

        Enum.GetNames(typeof(LogNames)).ToList().ForEach(async name =>{

            var queueName= $"direct-queue-{name}";
            await channel.QueueDeclareAsync(queueName,true,false,false);
            // await channel.QueueBindAsync(queueName,"logs-direct",routeKey);
        });

        Random rnd = new Random();
        Enumerable.Range(1,50).ToList().ForEach(async number =>{

            LogNames log1 = (LogNames)rnd.Next(1,5);
            LogNames log2 = (LogNames)rnd.Next(1,5);
            LogNames log3 = (LogNames)rnd.Next(1,5);

            //Defination of Topic
            var routeKey=$"{log1}.{log2}.{log3}";
            var message = $"log-type: {routeKey}";
            var body= Encoding.UTF8.GetBytes(message);
 
            await channel.BasicPublishAsync("logs-topic", routeKey,false, body);

            Console.WriteLine($"The log is sent: {message}");
        });

        Console.ReadLine();
    }
}