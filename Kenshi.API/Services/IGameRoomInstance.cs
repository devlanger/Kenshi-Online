using Kenshi.Shared.Models;

namespace Kenshi.API.Services;

public interface IGameRoomInstance
{
    bool Started { get; set; }
    bool TestServer { get; set; }
    
    string RoomNumber { get; set; }
    string LeaderUsername { get; set; }
    int Port { get; set; }
    int MaxPlayers { get; }
    int PlayersCount { get; }
    string RoomId { get; set; }
    string DisplayName { get; set; }
    List<string> Players { get; set; }
    RoomSettingsDto Settings { get; set; }
    void AddPlayer(string username);
    void RemovePlayer(string username);
    public GameRoomDto GetDto();
    void SetLeader(string roomPlayer);
}