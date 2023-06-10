using System.Text.RegularExpressions;
using Kenshi.API.Services;
using Kenshi.Shared.Models;

namespace Kenshi.API;

public class GameRoomInstance : IGameRoomInstance
{
    public bool TestServer { get; set; }
    public string RoomNumber { get; set; }
    public string RoomId { get; set; }
    public string DisplayName { get; set; }
    public string LeaderUsername { get; set; }
    public List<string> Players { get; set; } = new List<string>();
    public bool Started { get; set; }
    public int Port { get; set; }
    public int MaxPlayers => Settings.playersAmount;
    public int PlayersCount => Players.Count;
    public RoomSettingsDto Settings { get; set; } = new RoomSettingsDto();

    public GameRoomDto GetDto() => new GameRoomDto
    {
        roomNumber = RoomNumber,
        displayName = DisplayName,
        port = Port.ToString(),
        started = Started,
        players = Players,
        leaderUsername = LeaderUsername,
        settings = Settings
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