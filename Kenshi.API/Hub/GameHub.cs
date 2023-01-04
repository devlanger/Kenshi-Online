using k8s.Models;
using Kenshi.API.Helpers;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;

namespace Kenshi.API.Hub;

public class GameHub : Microsoft.AspNetCore.SignalR.Hub
{
    private readonly KubernetesService _service;

    public GameHub(KubernetesService service)
    {
        _service = service;
    }

    public async Task DeleteGameRoom(int port)
    {
        try
        {
            Console.WriteLine($"Delete game on port {port}");
            await _service.DeletePod(port);
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
        var podsList = _service.ListPods().Result.Select(p => p.Name()).ToList();
        await Clients.Client(Context.ConnectionId).SendAsync("ListGameRooms", JsonConvert.SerializeObject(podsList));
    }

    public Task JoinGameRoom(string roomId)
    {
        try
        {
            Console.WriteLine($"{Context.ConnectionId} has joined room");
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }

        return Task.CompletedTask;
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