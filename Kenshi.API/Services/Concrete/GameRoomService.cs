using Kenshi.API.Helpers;
using Kenshi.Shared.Models;

namespace Kenshi.API.Services;

public class GameRoomService : IGameRoomService
{
    private readonly KubernetesService _dockerService;
    public static Dictionary<string, IGameRoomInstance> Rooms = new Dictionary<string, IGameRoomInstance>();
    private ILogger<GameRoomService> _logger;

    public GameRoomService(KubernetesService dockerService, ILogger<GameRoomService> logger)
    {
        _dockerService = dockerService;
        _logger = logger;
    }

    public void AddRoom(IGameRoomInstance room)
    {
        Console.WriteLine("Added room to game room service");
        Rooms.Add(room.RoomId, room);
    }

    public void StartGameInstance(IGameRoomInstance roomData)
    {
        try
        {
            var pod = _dockerService.CreatePod(new GameRoomPodSettings
            {
                Port = roomData.Port, 
                MapName = roomData.Settings.mapName,
                GameModeType = roomData.GameType
            });
                    
            if (pod.Result)
            {
                roomData.Started = true;
            }
            else
            {
                _logger.LogError("Couldn't start pod.");
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e.ToString());
            throw;
        }
    } 
    
    public List<IGameRoomInstance> GetRooms()
    {
        return Rooms.Values.ToList();
    }

    public int GetFreePort()
    {
        int startPort = 5000;
        int freePort = startPort + 1;
        
        if (Rooms.Any())
        {
            // If there are pods in the cluster, find the highest used port number and add 1
            freePort = Rooms.Values.Max(c => c.Port + 1);
        }

        return freePort;
    }
    
    public IGameRoomInstance GetRoomByName(string roomId)
    {
        if (!Rooms.ContainsKey(roomId))
        {
            return null;
        }
        
        return Rooms[roomId];
    }
    
    public List<string> GetUsernamesInRoom(string roomName)
    {
        return GetRoomByName(roomName)?.Players.ToList();
    }
    
    public void AddPlayerToRoom(string roomName, string username)
    {
        GetRoomByName(roomName)?.AddPlayer(username);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="username"></param>
    /// <returns>Room Id</returns>
    public string RemovePlayerFromRoom(string username)
    {
        var room = GetRoomForUsername(username);
        if (room != null)
        {
            room.RemovePlayer(username);
            
            if (room.Players.Count == 1)
            {
                room.SetLeader(room.Players[0]);
            }
            
            if (room.Players.Count == 0 && !room.TestServer)
            {
                RemoveRoom(room.RoomId);
            }

            return room.RoomId;
        }

        return "";
    }

    public void RemoveRoom(string id)
    {
        Console.WriteLine($"Removed room {id}");
        if (Rooms[id].Started)
        {
            _dockerService.DeletePod(id);
        }
        Rooms.Remove(id);
    }

    public IGameRoomInstance GetRoomForUsername(string playerName)
    {
        return Rooms.Values.Select(r => r).FirstOrDefault(r => r.Players.Contains(playerName));
    }

    public IGameRoomInstance GetRoom(string dtoRoomId)
    {
        return Rooms.ContainsKey(dtoRoomId) ? Rooms[dtoRoomId] : null;
    }

    public IGameRoomInstance CreateRoom(string name, GameType type, bool isTest)
    {
        int port = GetFreePort();
        var room = new GameRoomInstance()
        {
            RoomNumber = port.ToString(),
            DisplayName = name,
            Port = port,
            Started = false,
            RoomId = KubernetesService.GetPodName(port),
            TestServer = isTest,
            GameType = type
        };
        AddRoom(room);
        
        return room;
    }
}