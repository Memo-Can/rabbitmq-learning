using System.Linq;
using System.Text;
using Microsoft.Extensions.Configuration;
using RabbitMQ.Client;

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
        var channel = await connection.CreateChannelAsync();
        
        //Exchange declarated as Fanout
        await channel.ExchangeDeclareAsync("logs-fanout",type: ExchangeType.Fanout,durable:true);

        Enumerable.Range(1,150).ToList().ForEach(async number =>{

            await channel.BasicPublishAsync("logs-fanout", string.Empty,false, Encoding.UTF8.GetBytes($"Log {number}"));

            Console.WriteLine($"Log: {number}");
        });

        Console.ReadLine();
    }
}