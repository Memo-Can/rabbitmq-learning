using System.Linq;
using System.Text;
using System.Text.Json;
using ClassLibrary;
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
        
        await channel.ExchangeDeclareAsync("header-exchange",type: ExchangeType.Headers,durable:true);

        var user = new User{
            Id=Guid.NewGuid(),
            Name="John",
            SureName="Someoneelse",
            Number=10
        };
        
        //Almost every type datas be able to sent with serlization
        var userSeriliezed = JsonSerializer.Serialize(user);

        await channel.BasicPublishAsync("header-exchange",string.Empty,false,Encoding.UTF8.GetBytes(userSeriliezed));

        Console.WriteLine("The Message has been sent");
        Console.ReadLine();
    }
}