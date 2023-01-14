namespace Kenshi.API.Services;

public class GameRoomService : IGameRoomService
{
    public static Dictionary<string, IGameRoomInstance> Rooms = new Dictionary<string, IGameRoomInstance>();

    public GameRoomService()
    {
        
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

    public void RemovePlayerFromRoom(string roomName, string username)
    {
        GetRoomByName(roomName)?.RemovePlayer(username);
    }

    public void RemoveRoom(string id)
    {
        Rooms.Remove(id);
    }

    public IGameRoomInstance GetRoomForUsername(string playerName)
    {
        return Rooms.Values.Select(r => r).FirstOrDefault(r => r.Players.Contains(playerName));
    }
}