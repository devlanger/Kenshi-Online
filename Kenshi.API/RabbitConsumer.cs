using System.Text;
using Kenshi.API.Services;
using Kenshi.Backend.Shared.Models;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Kenshi.API;

public class RabbitConsumer : IHostedService
{
    private readonly IConfiguration _configuration;
    private readonly IGameRoomService _gameRoomService;
    private IConnection _connection;
    private IModel _connectedChannel;
    private IModel _disconnectedChannel;
    private readonly string _host;

    public RabbitConsumer(IConfiguration config, IGameRoomService gameRoomService)
    {
        _configuration = config;
        _gameRoomService = gameRoomService;
        _host = _configuration["ConnectionStrings:rabbitmq"];
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        ConnectionFactory factory = null;
        
        for (int i = 0; i < 5; i++)
        {
            try
            {
                factory = new ConnectionFactory() { HostName = _host };
                _connection = factory.CreateConnection();
            }
            catch (Exception e)
            {
                Console.WriteLine($"Cant connect to rabbitmq [{_host}], retrying {i}/5");
                Thread.Sleep(5000);
            }
        }

        _connectedChannel = _connection.CreateModel();
        _connectedChannel.QueueDeclare(queue: "connected_user",
                             durable: false,
                             exclusive: false,
                             autoDelete: false,
                             arguments: null);

        _connectedChannel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);

        var connectedConsumer = new EventingBasicConsumer(_connectedChannel);
        connectedConsumer.Received += (model, ea) =>
        {
            try
            {
                var body = ea.Body;
                string json = Encoding.UTF8.GetString(ea.Body.ToArray());
                var dto = JsonConvert.DeserializeObject<UserRoomStateEventDto>(json);
                _gameRoomService.AddPlayerToRoom(dto.RoomId, dto.Username);

                Console.WriteLine($"{json}");
            
                _connectedChannel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        };
        _connectedChannel.BasicConsume(queue: "connected_user",
                             autoAck: false,
                             consumer: connectedConsumer);

        _disconnectedChannel = _connection.CreateModel();
        _disconnectedChannel.QueueDeclare(queue: "disconnected_user",
                             durable: false,
                             exclusive: false,
                             autoDelete: false,
                             arguments: null);

        _disconnectedChannel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);

        var disconnectedConsumer = new EventingBasicConsumer(_disconnectedChannel);
        disconnectedConsumer.Received += (model, ea) =>
        {
            try
            {
                var body = ea.Body;
                string json = Encoding.UTF8.GetString(ea.Body.ToArray());
                var dto = JsonConvert.DeserializeObject<UserRoomStateEventDto>(json);
                Console.WriteLine($"{json}");
                _gameRoomService.RemovePlayerFromRoom(dto.RoomId, dto.Username);

                _disconnectedChannel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        };
        _disconnectedChannel.BasicConsume(queue: "disconnected_user",
                             autoAck: false,
                             consumer: disconnectedConsumer);

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _disconnectedChannel.Close();
        _connectedChannel.Close();
        _connection.Close();

        return Task.CompletedTask;
    }
}