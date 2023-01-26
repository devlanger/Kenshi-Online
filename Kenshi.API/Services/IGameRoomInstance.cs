namespace Kenshi.API.Services;

public interface IGameRoomInstance
{
    int Port { get; set; }
    int MaxPlayers { get; set; }
    int PlayersCount { get; }
    string RoomId { get; set; }
    List<string> Players { get; set; }
    void AddPlayer(string username);
    void RemovePlayer(string username);
}