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
        
        //Exchange declarated as Direct
        await channel.ExchangeDeclareAsync("logs-direct",type: ExchangeType.Direct,durable:true);

        Enum.GetNames(typeof(LogNames)).ToList().ForEach(async name =>{

            var routeKey=$"route-{name}";
            var queueName= $"direct-queue-{name}";
             await channel.QueueDeclareAsync(queueName,true,false,false);
             await channel.QueueBindAsync(queueName,"logs-direct",routeKey);
        });


        Enumerable.Range(1,50).ToList().ForEach(async number =>{

            LogNames log = (LogNames)new Random().Next(1,5);

            var message = $"log-type: {log}";
            var body= Encoding.UTF8.GetBytes(message);
            var routeKey=$"route-{log}";

            await channel.BasicPublishAsync("logs-direct", routeKey,false, body);

            Console.WriteLine($"The log is sent: {message}");
        });

        Console.ReadLine();
    }
}