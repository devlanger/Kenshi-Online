using Kenshi.API.Services;

namespace Kenshi.API;

public class GameRoomInstance : IGameRoomInstance
{
    public string RoomId { get; set; }
    public string DisplayName { get; set; }
    public List<string> Players { get; set; } = new List<string>();
    public int Port { get; set; }
    public int MaxPlayers { get; set; }
    public int PlayersCount => Players.Count;

    public void AddPlayer(string username)
    {
        Players.Add(username);
    }

    public void RemovePlayer(string username)
    {
        Players.Remove(username);
    }
}