using Kenshi.API.Models;
using Kenshi.API.Models.Concrete;
using Microsoft.AspNetCore.SignalR;

namespace Kenshi.API.Services;

public class GameUserService
{
    /// <summary>
    /// <username, playerId>
    /// </summary>
    public static Dictionary<string, GameUser> userIds = new Dictionary<string, GameUser>();

    public static List<string> UsersInLobby = new List<string>();
    public static List<string> LoggedUsers = new List<string>();

    public class GameUser
    {
        public string Id { get; set; }
        
        public string Username { get; set; }
        
        public Dictionary<int, int> Customization { get; set; }
        
        public Lobby Lobby { get; set; }
        
        public string ConnectionId { get; set; }

        public PlayerConnection Connection { get; set; }
        
        public UserDto? User { get; set; }

        public bool IsAuthenticated => User is not null;
        public int Rating { get; set; }

        public GameUser()
        {
            Connection = new PlayerConnection();
            Customization = new Dictionary<int, int>();
        }
    }
    
    public static bool IsUserLogged(string username)
    {
        return LoggedUsers.Contains(username);
    }
    
    public string GetConnectionIdByUsername(string username)
    {
        if (!userIds.ContainsKey(username))
        {
            return "";
        }
        return userIds[username].Id;
    }

    public GameUser? GetUserByConnectionId(string id)
    {
        return userIds.Values.FirstOrDefault(u => u.Id == id);
    }
}