using System.Collections;
using Kenshi.Shared.Models;

namespace Kenshi.API.Services;

public interface IGameRoomService
{
    public int GetFreePort();
    void StartGameInstance(IGameRoomInstance roomData);
    void AddRoom(IGameRoomInstance room);
    List<IGameRoomInstance> GetRooms();
    List<string> GetUsernamesInRoom(string roomName);
    void AddPlayerToRoom(string roomName, string username);
    string RemovePlayerFromRoom(string username);
    void RemoveRoom(string id);
    IGameRoomInstance GetRoomForUsername(string playerName);
    IGameRoomInstance GetRoom(string dtoRoomId);
    IGameRoomInstance CreateRoom(string name, GameType type, bool isTest);
}