using Kenshi.API.Services;

namespace Kenshi.API;

public class GameRoomInstance : IGameRoomInstance
{
    public string RoomId { get; set; }
    public List<string> Players { get; set; } = new List<string>();
    public void AddPlayer(string username)
    {
        Players.Add(username);
    }

    public void RemovePlayer(string username)
    {
        Players.Remove(username);
    }
}