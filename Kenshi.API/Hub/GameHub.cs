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
    private readonly UserService _userService;
    private readonly MetricsService _metricsService;
    private readonly ILogger<GameHub> _logger;
    private readonly ConnectionMultiplexer redis;

    public static string RedisString(IConfiguration config) =>
        Environment.GetEnvironmentVariable("REDIS_HOST") ?? config["ConnectionStrings:redis"];
    
    private string GetUsername() => (string)Context.Items["username"];
    
    public GameHub(KubernetesService service,
        IConfiguration config,
        JwtTokenService tokenService,
        IGameRoomService gameRoomService,
        UserService userService,
        MetricsService metricsService,
        ILogger<GameHub> logger)
    {   
        redis = ConnectionMultiplexer.Connect(RedisString(config));

        _service = service;
        _config = config;
        _tokenService = tokenService;
        _gameRoomService = gameRoomService;
        _userService = userService;
        _metricsService = metricsService;
        _logger = logger;
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
        var podsList = new List<ContainerDto>();
        foreach (var item in _gameRoomService.GetRooms())
        {
            var dto = new ContainerDto()
            {
                Id = item.RoomId,
                Name = item.RoomId,
                Port = item.Port.ToString(),
                PlayersCount = item.Players.Count,
                MaxPlayersCount = item.MaxPlayers
            };
            
            podsList.Add(dto);
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
        string msg = "";
        string[] msgParams = message.Split(" ");
        switch (msgParams[0])
        {
            case "/w":
                if (msgParams.Length > 2)
                {
                    if (UserService.IsUserLogged(msgParams[1]))
                    {
                        int length = msgParams[0].Length + msgParams[1].Length;
                        string content = message.Substring(length, message.Length);
                        
                        msg = $"[PRIV] {GetUsername()}: {content}";
                        await Clients.Clients(GetUserConnectionIds(new List<string> {msgParams[1]})).SendAsync("ShowChatMessage", msg);
                        return;
                    }
                }
                break;
        }
        
        msg = $"{GetUsername()}: {message}";
        var roomInstance = _gameRoomService.GetRoomForUsername(GetUsername());
        
        if (roomInstance != null)
        {
            var users = _gameRoomService.GetUsernamesInRoom(roomInstance.RoomId);
            var ids = GetUserConnectionIds(users);
            Console.WriteLine($"[Room: {roomInstance.RoomId}] send message {message}");

            foreach (var u in ids)
            {
                Console.WriteLine($"id: {u}");
            }
            foreach (var u in users)
            {
                Console.WriteLine($"{u}");
            }

            await Clients.Clients(ids).SendAsync("ShowChatMessage", msg);
        }
        else
        {
            Console.WriteLine("Send lobby message");
            await Clients.Clients(GetUserConnectionIds(UserService.UsersInLobby)).SendAsync("ShowChatMessage", msg);
        }
    }

    public static List<string>? GetUserConnectionIds(List<string> users)
    {
        if (users.Count == 0)
        {
            return new List<string>();
        }
        
        return UserService.userIds?.Where(v => users.Contains(v.Key))?.Select(v => v.Value)?.ToList();
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
            var port = _gameRoomService.GetRooms().FirstOrDefault(p => roomId == p.RoomId)?.Port.ToString();

            if (port != null)
            {
                Console.WriteLine($"{Context.ConnectionId} has joined Game Room port: {port}");
                await Clients.Client(Context.ConnectionId).SendAsync("JoinGameRoom", port);
                // foreach (var playerName in GetPlayersInRoom(port))
                // {
                //     Console.WriteLine(playerName);
                // }
            }
            else
            {
                Console.WriteLine($"Room with id {roomId} is not present in GameRoomService.");
                await Clients.Client(Context.ConnectionId).SendAsync("JoinGameRoom", 5001.ToString());
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    public async Task BroadcastLobbyUsersList()
    {
        await Clients.Clients(GetUserConnectionIds(UserService.UsersInLobby)).SendAsync("UpdatePlayersList",
            JsonConvert.SerializeObject(UserService.LoggedUsers));
    }
    
    public override async Task OnConnectedAsync()
    {
        var username = $"User-{new Random().Next(10000).ToString()}";
        Console.WriteLine($"{Context.ConnectionId}: {username} has joined");
        Context.Items["username"] = username;
        UserService.UsersInLobby.Add(username);
        UserService.LoggedUsers.Add(username);
        UserService.userIds[username] = Context.ConnectionId;
        var token = _tokenService.GenerateToken(GetUsername());
        await BroadcastLobbyUsersList();
        await Clients.Clients(GetUserConnectionIds(UserService.UsersInLobby)).SendAsync("ShowChatMessage", $"[SYS] {username} has joined lobby");
        await Clients.Client(Context.ConnectionId).SendAsync("SetConnectionData", JsonConvert.SerializeObject(new ConnectionDto
        {
            token = token,
            nickname = username
        }));
        
        _metricsService.SetPlayersCount(UserService.LoggedUsers.Count);
        _logger.LogInformation($"{GetUsername()} has logged in.");
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        await Clients.Clients(GetUserConnectionIds(UserService.UsersInLobby)).SendAsync("ShowChatMessage", $"[SYS] {GetUsername()} has left lobby");
        UserService.UsersInLobby.Remove(GetUsername());
        UserService.LoggedUsers.Remove(GetUsername());
        await BroadcastLobbyUsersList();

        Console.WriteLine($"{Context.ConnectionId} has left");
        _metricsService.SetPlayersCount(UserService.LoggedUsers.Count);
        _logger.LogInformation($"{GetUsername()} has logged out.");
    }
}