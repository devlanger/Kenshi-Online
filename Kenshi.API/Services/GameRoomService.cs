using Kenshi.API.Helpers;

namespace Kenshi.API.Services;

public class GameRoomService : IGameRoomService
{
    private readonly KubernetesService _dockerService;
    public static Dictionary<string, IGameRoomInstance> Rooms = new Dictionary<string, IGameRoomInstance>();
    
    public GameRoomService(KubernetesService dockerService)
    {
        _dockerService = dockerService;
    }

    public void AddRoom(IGameRoomInstance room)
    {
        Console.WriteLine("Added room to game room service");
        Rooms.Add(room.RoomId, room);
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
            
            if (room.Players.Count == 0)
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

    public IGameRoomInstance CreateRoom(string name, bool isTest)
    {
        int port = GetFreePort();
        var room = new GameRoomInstance()
        {
            RoomNumber = port.ToString(),
            DisplayName = name,
            Port = port,
            Started = false,
            RoomId = KubernetesService.GetPodName(port),
            TestServer = isTest
        };
        AddRoom(room);
        
        return room;
    }
}