using System.Collections.Generic;

namespace Kenshi.Shared.Models
{
    public class GameRoomDto
    {
        public string port;
        public string mapId;
        public string leaderUsername;
        public bool started;
        public List<string> players;
    }
}