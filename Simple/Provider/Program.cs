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


        var deneme = configuration["AppSettings:Connection"];

        factory.Uri = new Uri(configuration["AppSettings:Connection"]);

        using var connection = await factory.CreateConnectionAsync();
        var channel = await connection.CreateChannelAsync();
        await channel.QueueDeclareAsync("queue-test", true, false, false);

        Enumerable.Range(1,20).ToList().ForEach(async number =>{

            await channel.BasicPublishAsync(string.Empty, "queue-test",false, Encoding.UTF8.GetBytes($"Message:{number}"));

            Console.WriteLine($"The Message: {number}");
        });

        Console.ReadLine();
    }
}