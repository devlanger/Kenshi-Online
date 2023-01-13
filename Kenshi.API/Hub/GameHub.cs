using Docker.DotNet.Models;
using k8s.Models;
using Kenshi.API.Helpers;
using Kenshi.API.Services;
using Kenshi.Shared.Models;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;
using StackExchange.Redis;

namespace Kenshi.API.Hub;

public class GameHub : Microsoft.AspNetCore.SignalR.Hub
{
    private readonly KubernetesService _service;
    private readonly IConfiguration _config;
    private readonly JwtTokenService _tokenService;
    private readonly IGameRoomService _gameRoomService;
    private readonly ConnectionMultiplexer redis;

    public static string RedisString(IConfiguration config) =>
        Environment.GetEnvironmentVariable("REDIS_HOST") ?? config["ConnectionStrings:redis"];
    
    private string GetUsername() => (string)Context.Items["username"];
    
    public GameHub(KubernetesService service, IConfiguration config, JwtTokenService tokenService, IGameRoomService gameRoomService)
    {   
        redis = ConnectionMultiplexer.Connect(RedisString(config));

        _service = service;
        _config = config;
        _tokenService = tokenService;
        _gameRoomService = gameRoomService;
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
            _gameRoomService.RemoveRoom(id);
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

            var gameRoomInstance = await _service.CreatePod(new GameRoomPodSettings()
            {
                Annotations = new Dictionary<string, string>(),
                Port = 3000,
                RoomId = 1
            });

            if (_gameRoomService != null)
            {
                _gameRoomService.AddRoom(gameRoomInstance);

                var podsList = await GetGamesRooms();
                await Clients.All.SendAsync("ListGameRooms", JsonConvert.SerializeObject(podsList.ToList()));
            }
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
        var podsList = await _service.ListPods();
        foreach (var item in podsList)
        {
            string playersCount = redis.GetDatabase().StringGet($"{item.Name}_players");
            item.PlayersCount = string.IsNullOrEmpty(playersCount) ? 0 : int.Parse(playersCount);
        }

        return podsList;
    }

    public async Task SendChatMessageToAllInRoom(string roomId, string message)
    {
        string msg = $"{GetUsername()}: {message}";
        Console.WriteLine($"send message {message}");
        
        await Clients.Users(GetPlayersInRoom(roomId)).SendAsync("ShowChatMessage", msg);
    }
    
    public async Task SendChatMessageToAll(string message)
    {
        string msg = $"{GetUsername()}: {message}";
        
        var roomInstance = _gameRoomService.GetRoomForUsername(GetUsername());
        
        if (roomInstance != null)
        {
            var users = _gameRoomService.GetUsernamesInRoom(roomInstance.RoomId);
            Console.WriteLine($"[Room: {roomInstance.RoomId}] send message {message}");
            foreach (var u in users)
            {
                Console.WriteLine($"{u}");
            }

            await Clients.Users(users).SendAsync("ShowChatMessage", msg);
        }
        else
        {
            await Clients.All.SendAsync("ShowChatMessage", msg);
        }
    }
    
    public List<string> GetPlayersInRoom(string port)
    {
        return redis.GetDatabase().ListRange($"gameroom-{port}").Select(x => x.ToString()).ToList();
    }

    public string GetRoomPortForPlayer(string playerName)
    {
        for (int i = 0; i < 10; i++)
        {
            var p = GetPlayersInRoom(i.ToString());
            var rp = p.FirstOrDefault(x => x == playerName);
            if (rp != null)
            {
                return i.ToString();
            }
        }

        return "-1";
    }
    
    public async Task JoinGameRoom(string roomId)
    {
        try
        {
            var port = _service.ListPods().Result.Find(p => roomId == p.Id).Port;

            Console.WriteLine($"{Context.ConnectionId} has joined room port: {port}");        
            await Clients.Client(Context.ConnectionId).SendAsync("JoinGameRoom", port);
            foreach (var playerName in GetPlayersInRoom(port))
            {
                Console.WriteLine(playerName);
            }
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
        Console.WriteLine($"{Context.ConnectionId}: {username} has joined");
        Context.Items["username"] = username;
        var token = _tokenService.GenerateToken(GetUsername());
        await Clients.All.SendAsync("ShowChatMessage", $"[SYS] {username} has joined");
        await Clients.Client(Context.ConnectionId).SendAsync("SetConnectionData", JsonConvert.SerializeObject(new ConnectionDto
        {
            token = token,
            nickname = username
        }));
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        await Clients.All.SendAsync("ShowChatMessage", $"[SYS] {GetUsername()} has left");
        Console.WriteLine($"{Context.ConnectionId} has left");
    }
}