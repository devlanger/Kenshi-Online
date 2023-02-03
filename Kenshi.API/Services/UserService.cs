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
        public string id;
        public Dictionary<int, int> customization = new Dictionary<int, int>();
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
        return userIds[username].id;
    }
}