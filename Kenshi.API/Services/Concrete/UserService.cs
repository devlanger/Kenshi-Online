using Kenshi.API.Models;
using Microsoft.AspNetCore.SignalR;

namespace Kenshi.API.Services;

public class UserService
{
    /// <summary>
    /// <username, playerId>
    /// </summary>
    public static Dictionary<string, User> userIds = new Dictionary<string, User>();

    public static List<string> UsersInLobby = new List<string>();
    public static List<string> LoggedUsers = new List<string>();

    public class User
    {
        public string Id { get; set; }
        
        public string Username { get; set; }
        
        public Dictionary<int, int> Customization { get; set; }
        
        public Lobby Lobby { get; set; }
        
        public string ConnectionId { get; set; }

        public User()
        {
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

    public User? GetUserByConnectionId(string id)
    {
        return userIds.Values.FirstOrDefault(u => u.Id == id);
    }
}