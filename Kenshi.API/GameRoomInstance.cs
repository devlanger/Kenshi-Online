using Kenshi.API.Services;
using Kenshi.Shared.Models;

namespace Kenshi.API;

public class GameRoomInstance : IGameRoomInstance
{
    public string RoomId { get; set; }
    public string DisplayName { get; set; }
    public string LeaderUsername { get; set; }
    public string MapId { get; set; } = "Map_1";
    public List<string> Players { get; set; } = new List<string>();
    public bool Started { get; set; }
    public int Port { get; set; }
    public int MaxPlayers { get; set; }
    public int PlayersCount => Players.Count;
    
    public GameRoomDto GetDto() => new GameRoomDto
    {
        port = Port.ToString(),
        started = Started,
        mapId = MapId,
        players = Players,
        leaderUsername = LeaderUsername
    };

    public void SetLeader(string roomPlayer)
    {
        LeaderUsername = roomPlayer;
    }

    public void AddPlayer(string username)
    {
        if (Players.Contains(username))
        {
            return;
        }
        
        UserService.UsersInLobby.Remove(username);
        Players.Add(username);
    }

    public void RemovePlayer(string username)
    {
        if (!Players.Contains(username))
        {
            return;
        }
        
        UserService.UsersInLobby.Add(username);
        Players.Remove(username);
    }
}