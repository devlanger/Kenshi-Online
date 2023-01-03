namespace Kenshi.API.Helpers;

public class GameRoomPodSettings
{
    public int RoomId { get; set; }
    public int Port { get; set; }
    
    public Dictionary<string, string> Annotations = new Dictionary<string, string>();
}