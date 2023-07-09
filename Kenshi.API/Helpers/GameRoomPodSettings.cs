using Kenshi.Shared.Models;

namespace Kenshi.API.Helpers;

public class GameRoomPodSettings
{
    public int Port { get; set; }
    public string MapName { get; set; }
    public GameType GameModeType { get; set; }
}