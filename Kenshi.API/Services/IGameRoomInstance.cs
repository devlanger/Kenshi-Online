namespace Kenshi.API.Services;

public interface IGameRoomInstance
{
    string RoomId { get; set; }
    List<string> Players { get; set; }
    void AddPlayer(string username);
    void RemovePlayer(string username);
}