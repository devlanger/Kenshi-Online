using k8s.Models;
using Kenshi.API.Helpers;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;
using StackExchange.Redis;

namespace Kenshi.API.Hub;

public class GameHub : Microsoft.AspNetCore.SignalR.Hub
{
    private readonly KubernetesService _service;
    
    public GameHub(KubernetesService service)
    {
        _service = service;
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
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    public async Task ListGameRooms()
    {
        var redis = ConnectionMultiplexer.Connect("redis");

        var podsList = await _service.ListPods();
        foreach (var item in podsList)
        {
            string playersCount = redis.GetDatabase().StringGet($"{item.Name}_players");
            item.PlayersCount = int.Parse(playersCount);
        }
        
        await Clients.Client(Context.ConnectionId).SendAsync("ListGameRooms", JsonConvert.SerializeObject(podsList.ToList()));
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

    public override Task OnConnectedAsync()
    {
        Console.WriteLine($"{Context.ConnectionId} has joined");
        return base.OnConnectedAsync();
    }

    public override Task OnDisconnectedAsync(Exception? exception)
    {
        Console.WriteLine($"{Context.ConnectionId} has left");
        return base.OnDisconnectedAsync(exception);
    }
}