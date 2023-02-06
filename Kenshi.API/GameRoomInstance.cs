using Kenshi.API.Services;
using Kenshi.Shared.Models;

namespace Kenshi.API;

public class GameRoomInstance : IGameRoomInstance
{
    public string RoomId { get; set; }
    public string DisplayName { get; set; }
    public string MapId { get; set; } = "Map_1";
    public List<string> Players { get; set; } = new List<string>();
    public int Port { get; set; }
    public int MaxPlayers { get; set; }
    public int PlayersCount => Players.Count;
    
    public bool Started = false;

    public GameRoomDto GetDto() => new GameRoomDto
    {
        port = Port.ToString(),
        started = Started,
        mapId = MapId
    };
    
    public void AddPlayer(string username)
    {
        Players.Add(username);
    }

    public void RemovePlayer(string username)
    {
        Players.Remove(username);
    }
}