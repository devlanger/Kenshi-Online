using System.Collections;

namespace Kenshi.API.Services;

public interface IGameRoomService
{
    void StartGameInstance(IGameRoomInstance roomData);
    void AddRoom(IGameRoomInstance room);
    List<IGameRoomInstance> GetRooms();
    List<string> GetUsernamesInRoom(string roomName);
    void AddPlayerToRoom(string roomName, string username);
    string RemovePlayerFromRoom(string username);
    void RemoveRoom(string id);
    IGameRoomInstance GetRoomForUsername(string playerName);
    IGameRoomInstance GetRoom(string dtoRoomId);
    IGameRoomInstance CreateRoom(string name, bool isTest);
}