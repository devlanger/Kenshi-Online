using Docker.DotNet.Models;
using k8s.Models;
using Kenshi.API.Helpers;
using Kenshi.API.Models;
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
    private readonly IMatchmakingService _matchmakingService;
    private readonly UserService _userService;
    private readonly MetricsService _metricsService;
    private readonly ILogger<GameHub> _logger;
    private readonly ConnectionMultiplexer redis;

    public const string CLIENT_VERSION = "0.11";
    
    public static string RedisString(IConfiguration config) =>
        Environment.GetEnvironmentVariable("REDIS_HOST") ?? config["ConnectionStrings:redis"];
    
    private string GetUsername() => (string)Context.Items["username"] ?? "Test-user";
    
    public GameHub(KubernetesService service,
        IConfiguration config,
        JwtTokenService tokenService,
        IGameRoomService gameRoomService,
        UserService userService,
        MetricsService metricsService,
        ILogger<GameHub> logger, 
        IMatchmakingService matchmakingService)
    {   
        redis = ConnectionMultiplexer.Connect(RedisString(config));

        _service = service;
        _config = config;
        _tokenService = tokenService;
        _gameRoomService = gameRoomService;
        _userService = userService;
        _metricsService = metricsService;
        _logger = logger;
        _matchmakingService = matchmakingService;
    }
    
    public async Task DeleteGameRoom(string id)
    {
        try
        {
            Console.WriteLine($"Delete game id {id}");
            await _service.DeletePod(id);
            _gameRoomService.RemoveRoom(id);
            
            await BroadcastRoomsList();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    private async Task BroadcastRoomsList()
    {
        var podsList = await GetGamesRooms();
        await Clients.All.SendAsync("ListGameRooms", JsonConvert.SerializeObject(podsList.ToList()));
    }

    public async Task CreateGameRoom(string name)
    {
        try
        {
            Console.WriteLine($"create game {name}");
            
            var gameRoomInstance = _gameRoomService.CreateRoom(name, false);

            gameRoomInstance.SetLeader(GetUsername());
            gameRoomInstance.AddPlayer(GetUsername());

            if (gameRoomInstance != null)
            {
                //_gameRoomService.AddRoom(gameRoomInstance);

                var podsList = await GetGamesRooms();
                
                await Clients.All.SendAsync("ListGameRooms", JsonConvert.SerializeObject(podsList.ToList()));
                await Clients.Clients(Context.ConnectionId).SendAsync("JoinGameRoom", JsonConvert.SerializeObject(gameRoomInstance.GetDto()));
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
                Name = item.DisplayName,
                Port = item.Port.ToString(),
                PlayersCount = item.Players.Count,
                MaxPlayersCount = item.MaxPlayers
            };
            
            podsList.Add(dto);
        }

        return podsList;
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
        
        return UserService.userIds?.Where(v => users.Contains(v.Key))?.Select(v => v.Value.Id)?.ToList();
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
    
    public async Task LeaveGameRoom()
    {
        try
        {
            string roomId = _gameRoomService.RemovePlayerFromRoom(GetUsername());
            await BroadcastRoomChangesToPlayers(_gameRoomService.GetRoom(roomId));
            Console.WriteLine($"{GetUsername()} has left Game Room {roomId}");
            
            await BroadcastRoomsList();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
    
    public async Task ChangeRoomSettings(string roomSettingsJson)
    {
        try
        {
            var roomData = _gameRoomService.GetRoomForUsername(GetUsername());

            if (roomData != null)
            {
                if (roomData.LeaderUsername == GetUsername())
                {
                    roomData.Settings = JsonConvert.DeserializeObject<RoomSettingsDto>(roomSettingsJson);
                    await BroadcastRoomChangesToPlayers(roomData);
                }
            }
            else
            {
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    
    public async Task StartMatchmaking()
    {
        var user = _userService.GetUserByConnectionId(Context.ConnectionId);
        if (user == null)
        {
            return;
        }
        
        _matchmakingService.StartLobbyMatchmaking(user.Lobby);
    }

    public async Task StopMatchmaking()
    {
        var user = _userService.GetUserByConnectionId(Context.ConnectionId);
        if (user == null)
        {
            return;
        }
        
        _matchmakingService.StopLobbyMatchmaking(user.Lobby);
        await Clients.Clients(user.Lobby.Users.Select(u => u.ConnectionId)).SendAsync("SetMatchmakingState", "false");
    }

    public async Task JoinGameRoom(string roomId)
    {
        try
        {
            var roomData = _gameRoomService.GetRooms().FirstOrDefault(p => roomId == p.RoomId);

            if (roomData != null)
            {
                //Test Server = ignore lobby & join automatically
                if (roomData.TestServer)
                {
                    await Clients.Client(Context.ConnectionId).SendAsync("JoinGameInstance",
                        JsonConvert.SerializeObject(roomData.GetDto()));
                }
                
                if (_gameRoomService.GetRoomForUsername(GetUsername()) != null)
                {
                    Console.WriteLine($"{GetUsername()} already in room!");
                    return;
                }
                
                Console.WriteLine($"{Context.ConnectionId} has joined Game Room port: {roomData.RoomId}");
                roomData.AddPlayer(GetUsername());
                await BroadcastRoomChangesToPlayers(roomData);
            }
            else
            {
                Console.WriteLine($"Room with id {roomId} is not present in GameRoomService.");
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    private async Task BroadcastRoomChangesToPlayers(IGameRoomInstance gameRoomInstance)
    {
        if(gameRoomInstance == null)
        {
            return;
        }
        
        var users = _gameRoomService.GetUsernamesInRoom(gameRoomInstance.RoomId);
        var ids = GetUserConnectionIds(users);

        string dto = JsonConvert.SerializeObject(gameRoomInstance.GetDto());
        await Clients.Clients(ids).SendAsync("JoinGameRoom", dto);
    }

    public async Task JoinGameInstance()
    {
        try
        {
            var roomData = _gameRoomService.GetRoomForUsername(GetUsername());

            if (roomData != null)
            {
                if (!roomData.Started)
                {
                    if (roomData.LeaderUsername != GetUsername())
                    {
                        Console.WriteLine(
                            $"{GetUsername()} cant start this game! [NO_LEADER]");
                        return;
                    }
                    
                    var pod = _service.CreatePod(new GameRoomPodSettings
                    {
                        Port = roomData.Port, 
                        MapName = roomData.Settings.mapName
                    });
                    
                    if (pod.Result)
                    {
                        roomData.Started = true;
                    }
                }

                if (roomData.Started)
                {
                    Console.WriteLine(
                        $"{Context.ConnectionId} has joined Game Room Instance port: {roomData.RoomId}");
                    
                    await Clients.Client(Context.ConnectionId).SendAsync("JoinGameInstance",
                        JsonConvert.SerializeObject(roomData.GetDto()));
                }
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
        var remoteClientVersion = Context.GetHttpContext().Request.Headers["client_version"];
        if (remoteClientVersion != CLIENT_VERSION)
        {
            await Clients.Client(Context.ConnectionId).SendAsync("ShowConnectionMessage", $"Wrong client version [{remoteClientVersion}]. Download new one [{CLIENT_VERSION}]!");
            Context.Abort();
            return;
        }

        var username = $"User-{new Random().Next(10000).ToString()}";
        Console.WriteLine($"{Context.ConnectionId}: {username} has joined");
        Context.Items["username"] = username;
        UserService.UsersInLobby.Add(username);
        UserService.LoggedUsers.Add(username);
        var user = new UserService.User
        {
            Id = Context.ConnectionId,
            Username = username,
            Customization = new Dictionary<int, int>(),
            ConnectionId = Context.ConnectionId
        };
        
        UserService.userIds[username] = user;
        
        _matchmakingService.AddUserToLobby(user, new Lobby());
        
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
        var username = GetUsername();
        await Clients.Clients(GetUserConnectionIds(UserService.UsersInLobby)).SendAsync("ShowChatMessage", $"[SYS] {GetUsername()} has left lobby");
        _matchmakingService.RemoveUserFromLobby(UserService.userIds[username]);

        UserService.UsersInLobby.Remove(username);
        UserService.LoggedUsers.Remove(username);
        
        string roomId = _gameRoomService.RemovePlayerFromRoom(username);

        if (roomId != "")
        {
            await BroadcastRoomChangesToPlayers(_gameRoomService.GetRoom(roomId));
        }

        await BroadcastRoomsList();
        await BroadcastLobbyUsersList();

        Console.WriteLine($"{Context.ConnectionId} has left");
        _metricsService.SetPlayersCount(UserService.LoggedUsers.Count);
        _logger.LogInformation($"{GetUsername()} has logged out.");
    }
}