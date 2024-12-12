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
        await channel.ExchangeDeclareAsync("header-exchange",type: ExchangeType.Headers,durable:true);

        //The header is a kind of dictionary and key is a string and value is a object that every data format be able to use.
        var props = new BasicProperties();
        props.Headers = new Dictionary<string, object>();
        props.Headers.Add("latitude",  51.5252949);
        props.Headers.Add("longitude", -0.0905493);
        
        //with that persistent, the message is a kind of durabe and never deleted after restart
        props.Persistent = true;
        
        await channel.BasicPublishAsync("header-exchange",string.Empty,false, props,Encoding.UTF8.GetBytes("Header Message"));

        Console.WriteLine("The Message has been sent");
        Console.ReadLine();
    }
}