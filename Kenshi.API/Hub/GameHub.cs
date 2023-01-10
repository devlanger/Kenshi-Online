using k8s.Models;
using Kenshi.API.Helpers;
using Kenshi.Shared.Models;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;
using StackExchange.Redis;

namespace Kenshi.API.Hub;

public class GameHub : Microsoft.AspNetCore.SignalR.Hub
{
    private readonly KubernetesService _service;
    private readonly IConfiguration _config;

    private string GetUsername() => (string)Context.Items["username"];
    
    public GameHub(KubernetesService service, IConfiguration config)
    {
        _service = service;
        _config = config;
    }
    
    public async Task DeleteAllGameRooms()
    {
        await _service.DeleteAllPods();
    }
    
    public async Task DeleteGameRoom(string id)
    {
        try
        {
            Console.WriteLine($"Delete game id {id}");
            await _service.DeletePod(id);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
    
    public async Task CreateGameRoom(string name)
    {
        try
        {
            Console.WriteLine($"create game");

            await _service.CreatePod(new GameRoomPodSettings()
            {
                Annotations = new Dictionary<string, string>(),
                Port = 3000,
                RoomId = 1
            });
            
            var podsList = await GetGamesRooms();
            await Clients.All.SendAsync("ListGameRooms", JsonConvert.SerializeObject(podsList.ToList()));
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    public async Task ListGameRooms()
    {
        var podsList = await GetGamesRooms();

        await Clients.Client(Context.ConnectionId).SendAsync("ListGameRooms", JsonConvert.SerializeObject(podsList.ToList()));
    }

    private async Task<List<ContainerDto>> GetGamesRooms()
    {
        var redis = ConnectionMultiplexer.Connect(Environment.GetEnvironmentVariable("REDIS_HOST") ?? "localhost");

        var podsList = await _service.ListPods();
        foreach (var item in podsList)
        {
            string playersCount = redis.GetDatabase().StringGet($"{item.Name}_players");
            item.PlayersCount = string.IsNullOrEmpty(playersCount) ? 0 : int.Parse(playersCount);
        }

        return podsList;
    }

    public async Task SendChatMessageToAll(string message)
    {
        string msg = $"{GetUsername()}: {message}";
        Console.WriteLine($"send message {message}");
        await Clients.All.SendAsync("ShowChatMessage", msg);
    }

    private async Task SendMessageToUser(string id, string message)
    {
        await Clients.Client(id).SendAsync("ShowChatMessage", message);
    }

    public async Task JoinGameRoom(string roomId)
    {
        try
        {
            var port = _service.ListPods().Result.Find(p => roomId == p.Id).Port;

            Console.WriteLine($"{Context.ConnectionId} has joined room port: {port}");        
            await Clients.Client(Context.ConnectionId).SendAsync("JoinGameRoom", port);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    public override async Task OnConnectedAsync()
    {
        var username = $"User-{new Random().Next(10000).ToString()}";
        Context.Items["username"] = username;
        Console.WriteLine($"{Context.ConnectionId}: {username} has joined");
        await Clients.All.SendAsync("ShowChatMessage", $"[SYS] {username} has joined");
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        await Clients.All.SendAsync("ShowChatMessage", $"[SYS] {GetUsername()} has left");
        Console.WriteLine($"{Context.ConnectionId} has left");
    }
}