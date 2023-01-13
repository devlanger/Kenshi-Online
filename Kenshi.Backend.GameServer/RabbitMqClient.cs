using RabbitMQ.Client;

namespace Kenshi.Backend.GameServer;

public class RabbitMqClient
{
    private string url = "127.0.0.1";

    public void SendConnectedUser(string queue, string msg)
    {
        var factory = new ConnectionFactory() { HostName = $"{Environment.GetEnvironmentVariable("RABBIT_MQ_HOST") ?? url}" };
        using(var connection = factory.CreateConnection())
        using(var channel = connection.CreateModel())
        {
            channel.QueueDeclare(queue: queue,
                durable: false,
                exclusive: false,
                autoDelete: false,
                arguments: null);

            var body = System.Text.Encoding.UTF8.GetBytes(msg);

            channel.BasicPublish(exchange: "",
                routingKey: queue,
                basicProperties: null,
                body: body);
        }
    }
}