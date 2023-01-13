using System.Collections;

namespace Kenshi.API.Services;

public interface IGameRoomService
{
    void AddRoom(IGameRoomInstance room);
    List<IGameRoomInstance> GetRooms();
    List<string> GetUsernamesInRoom(string roomName);
    void AddPlayerToRoom(string roomName, string username);
    void RemovePlayerFromRoom(string roomName, string username);
    void RemoveRoom(string id);
    IGameRoomInstance GetRoomForUsername(string playerName);
}